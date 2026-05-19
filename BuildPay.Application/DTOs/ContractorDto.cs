using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Application.DTOs;

public sealed class ContractorDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
}

