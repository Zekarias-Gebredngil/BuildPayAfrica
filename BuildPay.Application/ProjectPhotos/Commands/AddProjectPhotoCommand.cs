using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.ProjectPhotos.Commands;

public sealed record AddProjectPhotoCommand(Guid ProjectId, string Url, string? Caption) : IRequest<Guid>;

public sealed class AddProjectPhotoCommandValidator : AbstractValidator<AddProjectPhotoCommand>
{
    public AddProjectPhotoCommandValidator()
    {
        RuleFor(v => v.ProjectId).NotEmpty();
        RuleFor(v => v.Url).NotEmpty().MaximumLength(1000);
        RuleFor(v => v.Caption).MaximumLength(200);
    }
}

public sealed class AddProjectPhotoCommandHandler : IRequestHandler<AddProjectPhotoCommand, Guid>
{
    private readonly IBuildPayDbContext _context;

    public AddProjectPhotoCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddProjectPhotoCommand request, CancellationToken cancellationToken)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new ValidationException("Project not found.");
        }

        var entity = new ProjectPhoto
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Url = request.Url.Trim(),
            Caption = string.IsNullOrWhiteSpace(request.Caption) ? null : request.Caption.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectPhotos.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
