namespace CourierPlatform.Api.Features.CreateShipment;

public class IdempotencyRecord
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string PayloadHash { get; set; } = string.Empty;
    public long ShipmentId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Shipment? Shipment { get; set; }
}