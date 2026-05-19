using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Domain.Entities;

public class Contractor
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public List<ContractorDocument> Documents { get; set; } = new();
}
