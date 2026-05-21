namespace BuildPay.Contracts;

public static class IdentityEventTypes
{
    public const string ContractorVerified = "Identity.ContractorVerified";
    public const string ContractorRejected = "Identity.ContractorRejected";
}

public sealed record IdentityContractorVerified(
    Guid EventId,
    string Source,
    DateTime TimestampUtc,
    Guid UserId,
    Guid ContractorVerificationId
) : DomainEventBase(IdentityEventTypes.ContractorVerified, EventId, Source, TimestampUtc)
{
    public static IdentityContractorVerified Create(string source, Guid userId, Guid contractorVerificationId) =>
        new(EventId: NewId(), Source: source, TimestampUtc: UtcNow(), UserId: userId, ContractorVerificationId: contractorVerificationId);
}

public sealed record IdentityContractorRejected(
    Guid EventId,
    string Source,
    DateTime TimestampUtc,
    Guid UserId,
    string Reason
) : DomainEventBase(IdentityEventTypes.ContractorRejected, EventId, Source, TimestampUtc)
{
    public static IdentityContractorRejected Create(string source, Guid userId, string reason) =>
        new(EventId: NewId(), Source: source, TimestampUtc: UtcNow(), UserId: userId, Reason: reason);
}

