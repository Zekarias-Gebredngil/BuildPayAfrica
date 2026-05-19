using System;

namespace BuildPay.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public User Supplier { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;
}
