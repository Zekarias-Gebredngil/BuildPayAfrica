using System;

namespace BuildPay.Domain.Entities;

public class DeliveryLocationUpdate
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public Delivery Delivery { get; set; } = null!;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}

