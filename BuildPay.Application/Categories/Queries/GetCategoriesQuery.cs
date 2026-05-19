using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.DTOs;
using BuildPay.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Categories.Queries;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IBuildPayDbContext _context;

    public GetCategoriesQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync(cancellationToken);
    }
}
