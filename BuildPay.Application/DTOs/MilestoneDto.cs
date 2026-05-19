using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Application.DTOs;

public sealed class MilestoneDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public MilestoneStatus Status { get; set; }
    public DateTime DueDate { get; set; }
}

