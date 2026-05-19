using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.DTOs;
using BuildPay.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Products.Queries;

public record GetProductsQuery : IRequest<List<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly BuildPayDbContext _context;

    public GetProductsQueryHandler(BuildPayDbContext context)
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
