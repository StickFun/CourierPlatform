using CourierPlatform.Api.Features.CreateShipment;
using CourierPlatform.Api.Features.CreateShipment.Persistence;
using CourierPlatform.Api.Infrastructure.Messaging;
using CourierPlatform.Api.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CourierPlatform.Api.Infrastructure.Persistence;

/// <summary>
/// Create migration:
/// dotnet ef migrations add MIGRATIONNAME --project src/CourierPlatform.Api -o Infrastructure/Persistence/Migrations
///
/// Apply migration:
/// dotnet ef database update --project src/CourierPlatform.Api
/// </summary>
/// <param name="options"></param>
public class CourierPlatformDbContext(DbContextOptions<CourierPlatformDbContext> options)
    : DbContext(options)
{
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ShipmentConfiguration());
        modelBuilder.ApplyConfiguration(new IdempotencyRecordConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}
