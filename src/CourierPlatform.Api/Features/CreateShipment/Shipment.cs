namespace CourierPlatform.Api.Features.CreateShipment;

public class Shipment
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderPhone { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string PackageDescription { get; set; } = string.Empty;
    public int WeightGrams { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<IdempotencyRecord> IdempotencyRecords { get; set; } = [];
}
