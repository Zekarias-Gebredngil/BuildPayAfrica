using System;

namespace BuildPay.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Guid CategoryId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
