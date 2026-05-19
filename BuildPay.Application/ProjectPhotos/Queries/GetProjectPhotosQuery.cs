using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.ProjectPhotos.Queries;

public sealed record GetProjectPhotosQuery(Guid ProjectId) : IRequest<List<ProjectPhotoDto>>;

public sealed class GetProjectPhotosQueryHandler : IRequestHandler<GetProjectPhotosQuery, List<ProjectPhotoDto>>
{
    private readonly IBuildPayDbContext _context;

    public GetProjectPhotosQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectPhotoDto>> Handle(GetProjectPhotosQuery request, CancellationToken cancellationToken)
    {
        return await _context.ProjectPhotos
            .Where(p => p.ProjectId == request.ProjectId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectPhotoDto
            {
                Id = p.Id,
                ProjectId = p.ProjectId,
                Url = p.Url,
                Caption = p.Caption,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
