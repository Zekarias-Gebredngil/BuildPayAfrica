namespace BuildPay.Contracts;

public interface IDomainEvent
{
    string EventType { get; }
    Guid EventId { get; }
    string Source { get; }
    DateTime TimestampUtc { get; }
}

public abstract record DomainEventBase(
    string EventType,
    Guid EventId,
    string Source,
    DateTime TimestampUtc
) : IDomainEvent
{
    public static DateTime UtcNow() => DateTime.UtcNow;
    public static Guid NewId() => Guid.NewGuid();
}

