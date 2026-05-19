using System;

namespace BuildPay.Application.DTOs;

public sealed class ExpenseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

