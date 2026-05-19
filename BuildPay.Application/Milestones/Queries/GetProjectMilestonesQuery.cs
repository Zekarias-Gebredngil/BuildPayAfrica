using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Milestones.Queries;

public sealed record GetProjectMilestonesQuery(Guid ProjectId) : IRequest<List<MilestoneDto>>;

public sealed class GetProjectMilestonesQueryHandler : IRequestHandler<GetProjectMilestonesQuery, List<MilestoneDto>>
{
    private readonly IBuildPayDbContext _context;

    public GetProjectMilestonesQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<MilestoneDto>> Handle(GetProjectMilestonesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Milestones
            .Where(m => m.ProjectId == request.ProjectId)
            .OrderBy(m => m.DueDate)
            .Select(m => new MilestoneDto
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                Title = m.Title,
                Amount = m.Amount,
                Status = m.Status,
                DueDate = m.DueDate
            })
            .ToListAsync(cancellationToken);
    }
}
