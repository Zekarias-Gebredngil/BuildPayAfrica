namespace BuildPay.Contracts;

public static class PaymentEventTypes
{
    public const string MilestoneReleased = "Payment.MilestoneReleased";
}

public sealed record PaymentMilestoneReleased(
    Guid EventId,
    string Source,
    DateTime TimestampUtc,
    Guid ProjectId,
    Guid MilestoneId,
    Guid ContractorUserId,
    decimal Amount
) : DomainEventBase(PaymentEventTypes.MilestoneReleased, EventId, Source, TimestampUtc)
{
    public static PaymentMilestoneReleased Create(string source, Guid projectId, Guid milestoneId, Guid contractorUserId, decimal amount) =>
        new(EventId: NewId(), Source: source, TimestampUtc: UtcNow(), ProjectId: projectId, MilestoneId: milestoneId, ContractorUserId: contractorUserId, Amount: amount);
}

