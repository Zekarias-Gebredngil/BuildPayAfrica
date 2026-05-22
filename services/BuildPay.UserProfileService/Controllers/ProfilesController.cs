using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildPay.ServiceDefaults;
using System.Security.Claims;

namespace BuildPay.UserProfileService.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
public sealed class ProfilesController : ControllerBase
{
    private readonly UserProfileDbContext _db;

    public ProfilesController(UserProfileDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<ActionResult<object>> Me(CancellationToken ct)
    {
        var userId = GetUserId();
        var profile = await _db.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        return Ok(profile ?? new UserProfile { UserId = userId, Role = GetRole(), DisplayName = string.Empty });
    }

    [HttpPut("me")]
    public async Task<ActionResult> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null)
        {
            profile = new UserProfile { UserId = userId, Role = GetRole() };
            _db.Profiles.Add(profile);
        }

        profile.DisplayName = request.DisplayName ?? profile.DisplayName;
        profile.Bio = request.Bio ?? profile.Bio;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{userId:guid}/ratings")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> Rate([FromRoute] Guid userId, [FromBody] RateRequest request, CancellationToken ct)
    {
        if (request.Score is < 1 or > 5)
        {
            return BadRequest(new { message = "Score must be between 1 and 5." });
        }

        var rating = new UserRating
        {
            TargetUserId = userId,
            RaterUserId = GetUserId(),
            Score = request.Score,
            Comment = request.Comment
        };
        _db.Ratings.Add(rating);

        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null)
        {
            profile = new UserProfile { UserId = userId, Role = "Unknown" };
            _db.Profiles.Add(profile);
        }

        profile.RatingCount += 1;
        profile.RatingAverage = ((profile.RatingAverage * (profile.RatingCount - 1)) + request.Score) / profile.RatingCount;

        await _db.SaveChangesAsync(ct);
        return Ok(new { ratingId = rating.Id });
    }

    private Guid GetUserId()
    {
        return UserContext.GetUserId(User);
    }

    private string GetRole()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (roles.Count == 0) return string.Empty;

        // Prefer the app roles used throughout the platform.
        if (roles.Contains("Admin")) return "Admin";
        if (roles.Contains("Customer")) return "Customer";
        if (roles.Contains("Contractor")) return "Contractor";
        if (roles.Contains("Supplier")) return "Supplier";
        if (roles.Contains("Driver")) return "Driver";

        return roles.First();
    }

    public sealed record UpdateProfileRequest(string? DisplayName, string? Bio);
    public sealed record RateRequest(int Score, string? Comment);
}

