using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public string TransactionType { get; set; } = string.Empty;
}
