using CourierPlatform.Api.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierPlatform.Api.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.MessageId);

        builder.Property(x => x.MessageId)
            .ValueGeneratedNever();

        builder.Property(x => x.AggregateId)
            .IsRequired();

        builder.Property(x => x.AggregateVersion)
            .IsRequired();

        builder.Property(x => x.MessageType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(x => x.ProcessedAtUtc)
            .IsRequired(false);
    }
}
