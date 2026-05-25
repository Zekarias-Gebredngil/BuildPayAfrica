using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Contractors.Queries;

public sealed record GetPendingVerificationsQuery() : IRequest<List<PendingVerificationDto>>;

public sealed record PendingVerificationDto(
    Guid Id,
    Guid UserId,
    string? UserDisplayName,
    string? UserEmail,
    string LicenceNumber,
    List<string> DocumentUrls,
    DateTime SubmittedAt);

public sealed class GetPendingVerificationsQueryHandler : IRequestHandler<GetPendingVerificationsQuery, List<PendingVerificationDto>>
{
    private readonly IBuildPayDbContext _context;

    public GetPendingVerificationsQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<PendingVerificationDto>> Handle(GetPendingVerificationsQuery request, CancellationToken cancellationToken)
    {
        var contractors = await _context.Contractors
            .Include(c => c.User)
            .Include(c => c.Documents)
            .Where(c => c.VerificationStatus == VerificationStatus.Pending)
            .ToListAsync(cancellationToken);

        return contractors.Select(c => new PendingVerificationDto(
            c.Id,
            c.UserId,
            c.User?.FullName,
            c.User?.Email,
            c.LicenseNumber,
            c.Documents.Select(d => d.FileUrl).ToList(),
            DateTime.UtcNow // Replace with actual submitted date if tracked
        )).ToList();
    }
}
