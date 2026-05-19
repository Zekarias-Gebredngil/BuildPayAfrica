using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Application.DTOs;

public sealed class DeliveryDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid DriverId { get; set; }
    public DeliveryStatus Status { get; set; }
    public decimal? DeliveryFee { get; set; }
    public string? PickupAddress { get; set; }
    public string? DropoffAddress { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

