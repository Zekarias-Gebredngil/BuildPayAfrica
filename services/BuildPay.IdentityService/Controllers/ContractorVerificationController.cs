using BuildPay.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildPay.ServiceDefaults;
using System.Text.Json.Serialization;
namespace BuildPay.IdentityService.Controllers;

[ApiController]
[Route("api/contractors/verifications")]
public sealed class ContractorVerificationController : ControllerBase
{
    private readonly IdentityDbContext _db;
    private readonly IPublishEndpoint _publish;
    private readonly ILogger<ContractorVerificationController> _logger;

    public ContractorVerificationController(IdentityDbContext db, IPublishEndpoint publish, ILogger<ContractorVerificationController> logger)
    {
        _db = db;
        _publish = publish;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Contractor,Admin")]
    public async Task<ActionResult<object>> Submit([FromBody] SubmitVerificationRequest request, CancellationToken ct)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(request.LicenceNumber))
        {
            return BadRequest(new { message = "Licence number is required." });
        }

        if (request.DocumentUrls is null || request.DocumentUrls.Count == 0)
        {
            return BadRequest(new { message = "At least one document URL is required." });
        }

        var record = new ContractorVerification
        {
            UserId = userId,
            LicenceNumber = request.LicenceNumber.Trim(),
            Status = ContractorVerificationStatus.Pending,
            Documents = request.DocumentUrls.Select(u => new ContractorVerificationDocument { Url = u }).ToList()
        };

        _db.ContractorVerifications.Add(record);
        await _db.SaveChangesAsync(ct);

        return Ok(new { contractorVerificationId = record.Id });
    }

    [HttpGet("me")]
    [Authorize(Roles = "Contractor,Admin")]
    public async Task<ActionResult<object>> GetMyStatus(CancellationToken ct)
    {
        var userId = GetUserId();
        var record = await _db.ContractorVerifications
            .AsNoTracking()
            .Include(x => x.Documents)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (record is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = record.Id,
            status = record.Status,
            rejectionReason = record.RejectionReason,
            licenceNumber = record.LicenceNumber,
            documentUrls = record.Documents.Select(d => d.Url).ToList()
        });
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> Pending(CancellationToken ct)
    {
        var rows = await _db.ContractorVerifications
            .AsNoTracking()
            .Include(x => x.Documents)
            .Where(x => x.Status == ContractorVerificationStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(rows.Select(r => new
        {
            id = r.Id,
            userId = r.UserId,
            licenceNumber = r.LicenceNumber,
            status = r.Status,
            documentUrls = r.Documents.Select(d => d.Url).ToList()
        }));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var record = await _db.ContractorVerifications.Include(x => x.Documents).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (record is null)
        {
            return NotFound();
        }

        await ApplyStatusUpdate(record, request, ct);
        return NoContent();
    }

    // Convenience for admin tooling: approve/reject a contractor by userId without needing the verificationId.
    [HttpPut("by-user/{userId:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> UpdateStatusByUser([FromRoute] Guid userId, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var record = await _db.ContractorVerifications
            .Include(x => x.Documents)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (record is null)
        {
            return NotFound();
        }

        await ApplyStatusUpdate(record, request, ct);
        return Ok(new { contractorVerificationId = record.Id });
    }

    private async Task ApplyStatusUpdate(ContractorVerification record, UpdateStatusRequest request, CancellationToken ct)
    {
        record.UpdatedAtUtc = DateTime.UtcNow;

        IdentityContractorVerified? verifiedEvt = null;
        IdentityContractorRejected? rejectedEvt = null;

        if (request.Status == ContractorVerificationStatus.Approved)
        {
            record.Status = ContractorVerificationStatus.Approved;
            record.RejectionReason = null;

            verifiedEvt = IdentityContractorVerified.Create("BuildPay.IdentityService", record.UserId, record.Id);
        }
        else if (request.Status == ContractorVerificationStatus.Rejected)
        {
            record.Status = ContractorVerificationStatus.Rejected;
            record.RejectionReason = request.RejectionReason ?? string.Empty;

            rejectedEvt = IdentityContractorRejected.Create("BuildPay.IdentityService", record.UserId, record.RejectionReason);
        }
        else
        {
            record.Status = ContractorVerificationStatus.Pending;
            record.RejectionReason = null;
        }

        await _db.SaveChangesAsync(ct);

        if (verifiedEvt is not null)
        {
            await _publish.Publish(verifiedEvt, ct);
        }

        if (rejectedEvt is not null)
        {
            await _publish.Publish(rejectedEvt, ct);
        }
    }

    [HttpGet("/internal/stats/verifications/pending")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> PendingCount(CancellationToken ct)
    {
        var count = await _db.ContractorVerifications.CountAsync(x => x.Status == ContractorVerificationStatus.Pending, ct);
        return Ok(new { count });
    }

//     private Guid GetUserId()
//     {
//         return UserContext.GetUserId(User);
//     }

//     public sealed record SubmitVerificationRequest(string LicenceNumber, List<string> DocumentUrls);
//     public sealed record UpdateStatusRequest(ContractorVerificationStatus Status, string? RejectionReason);
// }
private Guid GetUserId()
    {
        return UserContext.GetUserId(User);
    }

    public sealed record SubmitVerificationRequest(string LicenceNumber, List<string> DocumentUrls);

    public sealed record UpdateStatusRequest(
        [property: JsonConverter(typeof(JsonStringEnumConverter))] 
        ContractorVerificationStatus Status, 
        string? RejectionReason
    );
}