using System;

namespace BuildPay.Domain.Entities;

public class ContractorReview
{
    public Guid Id { get; set; }
    public Guid ContractorId { get; set; }
    public Contractor Contractor { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

