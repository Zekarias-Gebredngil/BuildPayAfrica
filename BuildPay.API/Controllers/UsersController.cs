using BuildPay.API.Security;
using BuildPay.Domain.Entities;
using BuildPay.Domain.Enums;
using BuildPay.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly BuildPayDbContext _context;

    public UsersController(BuildPayDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<User>> GetOrCreateMe(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Id = userId,
                FullName = User.GetFullName() ?? string.Empty,
                Email = User.GetEmail() ?? string.Empty,
                PhoneNumber = User.GetPhoneNumber() ?? string.Empty,
                Role = GuessPrimaryRole(User),
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Ok(user);
    }

    private static Role GuessPrimaryRole(System.Security.Claims.ClaimsPrincipal principal)
    {
        var roleClaims = principal.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        foreach (var roleName in roleClaims)
        {
            if (Enum.TryParse<Role>(roleName, ignoreCase: true, out var role))
            {
                return role;
            }
        }

        return Role.Customer;
    }
}

