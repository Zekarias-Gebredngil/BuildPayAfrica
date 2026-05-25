using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Contractors.Queries;

public sealed record GetMyVerificationQuery(Guid UserId) : IRequest<VerificationStatusDto?>;

public sealed record VerificationStatusDto(Guid Id, string Status, string LicenceNumber, List<string> DocumentUrls, string? RejectionReason);

public sealed class GetMyVerificationQueryHandler : IRequestHandler<GetMyVerificationQuery, VerificationStatusDto?>
{
    private readonly IBuildPayDbContext _context;

    public GetMyVerificationQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<VerificationStatusDto?> Handle(GetMyVerificationQuery request, CancellationToken cancellationToken)
    {
        var contractor = await _context.Contractors
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (contractor == null)
        {
            return null;
        }

        return new VerificationStatusDto(
            contractor.Id,
            contractor.VerificationStatus switch {
                VerificationStatus.Pending => "Pending",
                VerificationStatus.Approved => "Approved",
                VerificationStatus.Rejected => "Rejected",
                _ => contractor.VerificationStatus.ToString()
            },
            contractor.LicenseNumber,
            contractor.Documents.Select(d => d.FileUrl).ToList(),
            null // Assuming RejectionReason is not currently persisted, or add if it is.
        );
    }
}
