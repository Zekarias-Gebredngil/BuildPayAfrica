using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Domain.Entities;

public class Milestone
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public MilestoneStatus Status { get; set; }
    public DateTime DueDate { get; set; }
}
