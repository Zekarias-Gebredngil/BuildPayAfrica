using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Entities;
using BuildPay.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Orders.Commands;

public sealed record CreateOrderCommand(Guid CustomerId, List<CreateOrderItem> Items) : IRequest<Guid>;

public sealed record CreateOrderItem(Guid ProductId, int Quantity);

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(v => v.CustomerId).NotEmpty();
        RuleFor(v => v.Items).NotEmpty();
        RuleForEach(v => v.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IBuildPayDbContext _context;

    public CreateOrderCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new ValidationException("One or more products do not exist.");
        }

        foreach (var item in request.Items)
        {
            var product = products.Single(p => p.Id == item.ProductId);
            if (product.Quantity < item.Quantity)
            {
                throw new ValidationException($"Insufficient stock for product '{product.Name}'.");
            }
        }

        var orderId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var orderItems = request.Items.Select(i =>
        {
            var product = products.Single(p => p.Id == i.ProductId);
            var unitPrice = product.Price;
            var lineTotal = unitPrice * i.Quantity;
            return new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = unitPrice,
                LineTotal = lineTotal
            };
        }).ToList();

        var total = orderItems.Sum(i => i.LineTotal);

        var order = new Order
        {
            Id = orderId,
            CustomerId = request.CustomerId,
            TotalAmount = total,
            Status = OrderStatus.Pending,
            CreatedAt = now,
            Items = orderItems
        };

        foreach (var item in request.Items)
        {
            var product = products.Single(p => p.Id == item.ProductId);
            product.Quantity -= item.Quantity;
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return orderId;
    }
}
