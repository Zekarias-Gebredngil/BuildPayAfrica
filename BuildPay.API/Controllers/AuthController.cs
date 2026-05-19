using BuildPay.Infrastructure.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IKeycloakAdminClient _keycloak;

    public AuthController(IKeycloakAdminClient keycloak)
    {
        _keycloak = keycloak;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var roleName = request.Role.Trim();
        var userId = await _keycloak.CreateUserAsync(new KeycloakCreateUserRequest
        {
            Username = request.Email.Trim(),
            Email = request.Email.Trim(),
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
        var token = await _keycloak.PasswordLoginAsync(request.Email.Trim(), request.Password, cancellationToken);
        return Ok(new
        {
            token = token.AccessToken,
            refreshToken = token.RefreshToken,
            expiresIn = token.ExpiresIn
        });
    }

    public sealed record RegisterRequest(string FullName, string Email, string Password, string Role);
    public sealed record LoginRequest(string Email, string Password);
}

