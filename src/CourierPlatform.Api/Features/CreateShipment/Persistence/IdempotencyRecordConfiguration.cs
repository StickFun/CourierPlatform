using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierPlatform.Api.Features.CreateShipment.Persistence;

public class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ClientId)
            .IsRequired();

        builder.Property(x => x.OperationName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.PayloadHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ShipmentId)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(x => new { x.ClientId, x.OperationName, x.IdempotencyKey })
            .IsUnique();
    }
}