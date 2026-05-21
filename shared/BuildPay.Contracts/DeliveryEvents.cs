namespace BuildPay.Contracts;

public static class DeliveryEventTypes
{
    public const string Updated = "Delivery.Updated";
    public const string Completed = "Delivery.Completed";
}

public sealed record DeliveryUpdated(
    Guid EventId,
    string Source,
    DateTime TimestampUtc,
    Guid DeliveryId,
    string Status
) : DomainEventBase(DeliveryEventTypes.Updated, EventId, Source, TimestampUtc)
{
    public static DeliveryUpdated Create(string source, Guid deliveryId, string status) =>
        new(EventId: NewId(), Source: source, TimestampUtc: UtcNow(), DeliveryId: deliveryId, Status: status);
}

public sealed record DeliveryCompleted(
    Guid EventId,
    string Source,
    DateTime TimestampUtc,
    Guid DeliveryId,
    Guid CustomerUserId
) : DomainEventBase(DeliveryEventTypes.Completed, EventId, Source, TimestampUtc)
{
    public static DeliveryCompleted Create(string source, Guid deliveryId, Guid customerUserId) =>
        new(EventId: NewId(), Source: source, TimestampUtc: UtcNow(), DeliveryId: deliveryId, CustomerUserId: customerUserId);
}

