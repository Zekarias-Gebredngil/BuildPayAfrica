using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Domain.Entities;

public class ContractorDocument
{
    public Guid Id { get; set; }
    public Guid ContractorId { get; set; }
    public Contractor Contractor { get; set; } = null!;
    public ContractorDocumentType DocumentType { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

