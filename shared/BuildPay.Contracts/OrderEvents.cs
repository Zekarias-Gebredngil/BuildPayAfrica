namespace BuildPay.Contracts;

public static class OrderEventTypes
{
    public const string Created = "Order.Created";
}

public sealed record OrderCreated(
    Guid EventId,
    string Source,
    DateTime TimestampUtc,
    Guid OrderId,
    Guid CustomerUserId
) : DomainEventBase(OrderEventTypes.Created, EventId, Source, TimestampUtc)
{
    public static OrderCreated Create(string source, Guid orderId, Guid customerUserId) =>
        new(EventId: NewId(), Source: source, TimestampUtc: UtcNow(), OrderId: orderId, CustomerUserId: customerUserId);
}

