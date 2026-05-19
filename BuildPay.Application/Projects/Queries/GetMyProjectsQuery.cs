using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Projects.Queries;

public sealed record GetMyProjectsQuery(Guid CustomerId) : IRequest<List<ProjectDto>>;

public sealed class GetMyProjectsQueryHandler : IRequestHandler<GetMyProjectsQuery, List<ProjectDto>>
{
    private readonly IBuildPayDbContext _context;

    public GetMyProjectsQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectDto>> Handle(GetMyProjectsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Projects
            .Where(p => p.CustomerId == request.CustomerId)
            .OrderByDescending(p => p.Name)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                ContractorId = p.ContractorId,
                Name = p.Name,
                Budget = p.Budget,
                Status = p.Status
            })
            .ToListAsync(cancellationToken);
    }
}
