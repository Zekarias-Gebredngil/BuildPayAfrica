using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Projects.Queries;

public sealed record GetProjectByIdQuery(Guid ProjectId) : IRequest<ProjectDto?>;

public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly IBuildPayDbContext _context;

    public GetProjectByIdQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Projects
            .Where(p => p.Id == request.ProjectId)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                ContractorId = p.ContractorId,
                Name = p.Name,
                Budget = p.Budget,
                Status = p.Status
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
