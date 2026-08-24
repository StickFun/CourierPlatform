using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierPlatform.Api.Features.CreateShipment.Persistence;

public class ShipmentConfiguration
    : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ClientId)
            .IsRequired();

        builder.Property(x => x.SenderName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SenderPhone)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.PickupAddress)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.RecipientName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.RecipientPhone)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.DeliveryAddress)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.PackageDescription)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.WeightGrams)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasMany(x => x.IdempotencyRecords)
            .WithOne(x => x.Shipment)
            .HasForeignKey(x => x.ShipmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.ClientId);
    }
}
