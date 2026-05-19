using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public Guid ContractorId { get; set; }
    public Contractor Contractor { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public ProjectStatus Status { get; set; }
}
