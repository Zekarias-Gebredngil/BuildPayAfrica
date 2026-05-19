using System;

namespace BuildPay.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

