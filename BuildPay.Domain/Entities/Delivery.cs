using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Domain.Entities;

public class Delivery
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid DriverId { get; set; }
    public User Driver { get; set; } = null!;
    public DeliveryStatus Status { get; set; }
    public decimal? DeliveryFee { get; set; }
    public string? PickupAddress { get; set; }
    public string? DropoffAddress { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

