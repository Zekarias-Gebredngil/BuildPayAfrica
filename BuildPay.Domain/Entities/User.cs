using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
