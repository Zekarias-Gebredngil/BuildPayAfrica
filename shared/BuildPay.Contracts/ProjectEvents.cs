namespace BuildPay.Contracts;

public static class ProjectEventTypes
{
    public const string MilestoneCompleted = "Project.MilestoneCompleted";
    public const string MilestoneApproved = "Project.MilestoneApproved";
}

public sealed record ProjectMilestoneCompleted(
    Guid EventId,
    string Source,
    DateTime TimestampUtc,
    Guid ProjectId,
    Guid MilestoneId,
    decimal Amount
) : DomainEventBase(ProjectEventTypes.MilestoneCompleted, EventId, Source, TimestampUtc)
{
    public static ProjectMilestoneCompleted Create(string source, Guid projectId, Guid milestoneId, decimal amount) =>
        new(EventId: NewId(), Source: source, TimestampUtc: UtcNow(), ProjectId: projectId, MilestoneId: milestoneId, Amount: amount);
}

public sealed record ProjectMilestoneApproved(
    Guid EventId,
    string Source,
    DateTime TimestampUtc,
    Guid ProjectId,
    Guid MilestoneId
) : DomainEventBase(ProjectEventTypes.MilestoneApproved, EventId, Source, TimestampUtc)
{
    public static ProjectMilestoneApproved Create(string source, Guid projectId, Guid milestoneId) =>
        new(EventId: NewId(), Source: source, TimestampUtc: UtcNow(), ProjectId: projectId, MilestoneId: milestoneId);
}

