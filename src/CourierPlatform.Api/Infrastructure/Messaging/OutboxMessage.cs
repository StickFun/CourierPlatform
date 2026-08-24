namespace CourierPlatform.Api.Infrastructure.Messaging;

public class OutboxMessage
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public long AggregateId { get; set; }
    public int AggregateVersion { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
}
