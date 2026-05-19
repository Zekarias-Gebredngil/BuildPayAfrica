using BuildPay.Application.Orders.Commands;
using BuildPay.Domain.Entities;
using BuildPay.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildPay.Application.Tests.Orders;

public sealed class CreateOrderCommandTests
{
    [Fact]
    public async Task Creates_order_with_items_and_decrements_stock()
    {
        await using var db = TestDbFactory.Create(nameof(Creates_order_with_items_and_decrements_stock));

        var customerId = Guid.NewGuid();
        db.Users.Add(new User { Id = customerId, FullName = "Customer", Email = "c@x.com", PhoneNumber = "", Role = Role.Customer, CreatedAt = DateTime.UtcNow });

        var supplierId = Guid.NewGuid();
        db.Users.Add(new User { Id = supplierId, FullName = "Supplier", Email = "s@x.com", PhoneNumber = "", Role = Role.Supplier, CreatedAt = DateTime.UtcNow });

        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "Cement", Description = "" });

        var productId = Guid.NewGuid();
        db.Products.Add(new Product { Id = productId, SupplierId = supplierId, Name = "Bag", Price = 10m, Quantity = 5, CategoryId = categoryId, ImageUrl = "" });

        await db.SaveChangesAsync();

        var handler = new CreateOrderCommandHandler(db);
        var orderId = await handler.Handle(
            new CreateOrderCommand(customerId, new List<CreateOrderCommand.CreateOrderItem> { new(productId, 2) }),
            CancellationToken.None);

        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Pending, order!.Status);
        Assert.Single(order.Items);
        Assert.Equal(20m, order.TotalAmount);

        var product = await db.Products.FirstAsync(p => p.Id == productId);
        Assert.Equal(3, product.Quantity);
    }

    [Fact]
    public async Task Throws_when_stock_insufficient()
    {
        await using var db = TestDbFactory.Create(nameof(Throws_when_stock_insufficient));

        var customerId = Guid.NewGuid();
        db.Users.Add(new User { Id = customerId, FullName = "Customer", Email = "c@x.com", PhoneNumber = "", Role = Role.Customer, CreatedAt = DateTime.UtcNow });

        var supplierId = Guid.NewGuid();
        db.Users.Add(new User { Id = supplierId, FullName = "Supplier", Email = "s@x.com", PhoneNumber = "", Role = Role.Supplier, CreatedAt = DateTime.UtcNow });

        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "Cement", Description = "" });

        var productId = Guid.NewGuid();
        db.Products.Add(new Product { Id = productId, SupplierId = supplierId, Name = "Bag", Price = 10m, Quantity = 1, CategoryId = categoryId, ImageUrl = "" });

        await db.SaveChangesAsync();

        var handler = new CreateOrderCommandHandler(db);

        await Assert.ThrowsAsync<ValidationException>(async () =>
            await handler.Handle(
                new CreateOrderCommand(customerId, new List<CreateOrderCommand.CreateOrderItem> { new(productId, 2) }),
                CancellationToken.None));
    }
}

