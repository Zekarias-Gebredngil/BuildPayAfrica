using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.DTOs;
using BuildPay.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Products.Queries;

public record GetProductsQuery : IRequest<List<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly IBuildPayDbContext _context;

    public GetProductsQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .Select(p => new ProductDto
            {
                Id = p.Id,
                SupplierId = p.SupplierId,
                Name = p.Name,
                Price = p.Price,
                Quantity = p.Quantity,
                CategoryId = p.CategoryId,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync(cancellationToken);
    }
}
