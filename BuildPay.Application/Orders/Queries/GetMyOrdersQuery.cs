using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Orders.Queries;

public sealed record GetMyOrdersQuery(Guid CustomerId) : IRequest<List<OrderDto>>;

public sealed class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, List<OrderDto>>
{
    private readonly IBuildPayDbContext _context;

    public GetMyOrdersQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
