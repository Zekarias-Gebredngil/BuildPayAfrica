using BuildPay.Infrastructure.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    // Roles that users are allowed to self-assign at registration.
    // Admin can only be assigned by an existing Admin — never self-registered.
    private static readonly HashSet<string> AllowedSelfRegistrationRoles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Customer", "Contractor", "Supplier", "Driver"
        };

    private readonly IKeycloakAdminClient _keycloak;

    public AuthController(IKeycloakAdminClient keycloak)
    {
        _keycloak = keycloak;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { message = "Full name is required." });

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            return BadRequest(new { message = "A valid email address is required." });

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters." });

        var roleName = request.Role?.Trim() ?? string.Empty;
        if (!AllowedSelfRegistrationRoles.Contains(roleName))
            return BadRequest(new { message = $"Invalid role. Allowed values: {string.Join(", ", AllowedSelfRegistrationRoles)}." });

        var userId = await _keycloak.CreateUserAsync(new KeycloakCreateUserRequest
        {
            Username = request.Email.Trim().ToLowerInvariant(),
            Email    = request.Email.Trim().ToLowerInvariant(),
            FirstName = request.FullName.Trim(),
            Enabled = true,
            EmailVerified = false,
            Credentials = new List<KeycloakCredential>
            {
                new() { Type = "password", Value = request.Password, Temporary = false }
            }
        }, cancellationToken);

        await _keycloak.AssignRealmRoleAsync(userId, roleName, cancellationToken);

        return Ok(new { userId });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        var token = await _keycloak.PasswordLoginAsync(request.Email.Trim().ToLowerInvariant(), request.Password, cancellationToken);
        return Ok(new
        {
            token        = token.AccessToken,
            refreshToken = token.RefreshToken,
            expiresIn    = token.ExpiresIn
        });
    }

    public sealed record RegisterRequest(string FullName, string Email, string Password, string Role);
    public sealed record LoginRequest(string Email, string Password);
}

