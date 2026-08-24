using CourierPlatform.Api.Features.CreateShipment;
using CourierPlatform.Api.Infrastructure.Messaging;
using CourierPlatform.Api.Infrastructure.Persistence;
using CourierPlatform.Api.IntegrationTests.Fixture;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CourierPlatform.Api.IntegrationTests.Infrastructure.Database;

public sealed class CourierPlatformDbContextTests(PostgreSqlFixture postgres) : IClassFixture<PostgreSqlFixture>
{
    private CourierPlatformDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<CourierPlatformDbContext>()
            .UseNpgsql(postgres.ConnectionString)
            .Options;

        return new CourierPlatformDbContext(options);
    }

    private CourierPlatformDbContext GetDbContextWithMigrations()
    {
        var dbContext = GetDbContext();
        dbContext.Database.Migrate();

        return dbContext;
    }

    private static Shipment CreateShipment(long clientId, string suffix) => new()
    {
        ClientId = clientId,
        SenderName = $"Sender {suffix}",
        SenderPhone = "+35799111222",
        PickupAddress = $"{suffix} Pickup Street, Nicosia",
        RecipientName = $"Recipient {suffix}",
        RecipientPhone = "+35799333444",
        DeliveryAddress = $"{suffix} Delivery Street, Limassol",
        PackageDescription = $"Parcel {suffix}",
        WeightGrams = 1250,
    };

    [Fact]
    public async Task ConnectsToDatabaseWithConnectionString()
    {
        await using var dbContext = GetDbContext();

        Assert.True(await dbContext.Database.CanConnectAsync());
    }

    [Fact]
    public async Task AppliesMigrationsToDatabase()
    {
        await using var dbContext = GetDbContext();

        await dbContext.Database.MigrateAsync();

        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        Assert.NotEmpty(appliedMigrations);
        Assert.Empty(pendingMigrations);
    }

    [Fact]
    public async Task ThrowsExceptionOnIdempotencyRecordWithExistingKey()
    {
        await using var dbContext = GetDbContextWithMigrations();

        var shipment = CreateShipment(2, "duplicate-key");

        await dbContext.Shipments.AddAsync(shipment);
        await dbContext.SaveChangesAsync();

        var idempotencyRecord1 = new IdempotencyRecord
        {
            ShipmentId = shipment.Id,
            ClientId = 2,
            OperationName = "CreateShipment",
            IdempotencyKey = "CreateShipment-12345",
            PayloadHash = "hash1",
        };

        var idempotencyRecord2 = new IdempotencyRecord
        {
            ShipmentId = shipment.Id,
            ClientId = 2,
            OperationName = "CreateShipment",
            IdempotencyKey = "CreateShipment-12345",
            PayloadHash = "hash2",
        };

        await dbContext.IdempotencyRecords.AddAsync(idempotencyRecord1);
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await dbContext.IdempotencyRecords.AddAsync(idempotencyRecord2);
            await dbContext.SaveChangesAsync();
        });

        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
        Assert.Equal(
            "IX_idempotency_records_ClientId_OperationName_IdempotencyKey",
            postgresException.ConstraintName);
    }

    [Fact]
    public async Task SavesDifferentLogicalOperationsWithDifferentKeys()
    {
        await using var dbContext = GetDbContextWithMigrations();

        var firstShipment = CreateShipment(3, "first-operation");
        var secondShipment = CreateShipment(3, "second-operation");

        await dbContext.Shipments.AddRangeAsync(firstShipment, secondShipment);
        await dbContext.SaveChangesAsync();

        var idempotencyRecord1 = new IdempotencyRecord
        {
            ShipmentId = firstShipment.Id,
            ClientId = 3,
            OperationName = "CreateShipment",
            IdempotencyKey = "CreateShipment-12345",
            PayloadHash = "hash1",
        };

        var idempotencyRecord2 = new IdempotencyRecord
        {
            ShipmentId = secondShipment.Id,
            ClientId = 3,
            OperationName = "CreateShipment",
            IdempotencyKey = "CreateShipment-67890",
            PayloadHash = "hash1",
        };

        await dbContext.IdempotencyRecords.AddAsync(idempotencyRecord1);
        await dbContext.IdempotencyRecords.AddAsync(idempotencyRecord2);
        await dbContext.SaveChangesAsync();

        Assert.NotEqual(idempotencyRecord1.ShipmentId, idempotencyRecord2.ShipmentId);
    }

    [Fact]
    public async Task DoesNotDeleteShipmentWithIdempotencyHistory()
    {
        long shipmentId;

        await using (var arrangeContext = GetDbContextWithMigrations())
        {
            var shipment = CreateShipment(4, "protected");
            shipment.IdempotencyRecords.Add(new IdempotencyRecord
            {
                ClientId = 4,
                OperationName = "CreateShipment",
                IdempotencyKey = "CreateShipment-protected",
                PayloadHash = "protected-hash",
            });

            arrangeContext.Shipments.Add(shipment);
            await arrangeContext.SaveChangesAsync();
            shipmentId = shipment.Id;
        }

        await using (var deleteContext = GetDbContext())
        {
            var shipment = await deleteContext.Shipments.SingleAsync(x => x.Id == shipmentId);
            deleteContext.Shipments.Remove(shipment);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => deleteContext.SaveChangesAsync());
            var postgresException = Assert.IsType<PostgresException>(exception.InnerException);

            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, postgresException.SqlState);
            Assert.Equal(
                "FK_idempotency_records_shipments_ShipmentId",
                postgresException.ConstraintName);
        }

        await using var assertContext = GetDbContext();
        Assert.True(await assertContext.Shipments.AnyAsync(x => x.Id == shipmentId));
        Assert.True(await assertContext.IdempotencyRecords.AnyAsync(x => x.ShipmentId == shipmentId));
    }

    [Fact]
    public async Task PersistsApplicationGeneratedOutboxMessageId()
    {
        await using var dbContext = GetDbContextWithMigrations();
        var message = new OutboxMessage
        {
            AggregateId = 42,
            AggregateVersion = 1,
            MessageType = "ShipmentCreated",
            Payload = "{\"shipmentId\":42}",
        };
        var messageId = message.MessageId;

        Assert.NotEqual(Guid.Empty, messageId);

        dbContext.OutboxMessages.Add(message);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();
        var savedMessage = await dbContext.OutboxMessages.SingleAsync(x => x.MessageId == messageId);
        Assert.Equal(messageId, savedMessage.MessageId);
    }
}
