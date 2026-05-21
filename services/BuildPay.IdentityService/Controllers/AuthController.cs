using BuildPay.Infrastructure.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.IdentityService.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private static readonly HashSet<string> AllowedSelfRegistrationRoles =
        new(StringComparer.OrdinalIgnoreCase) { "Customer", "Contractor", "Supplier", "Driver" };

    private readonly IKeycloakAdminClient _keycloak;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IKeycloakAdminClient keycloak, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _keycloak = keycloak;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Self-registration as Admin is not permitted." });
        }

        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length is < 1 or > 100)
        {
            return BadRequest(new { message = "Full name must be between 1 and 100 characters." });
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return BadRequest(new { message = "A valid email address is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length is < 8 or > 128)
        {
            return BadRequest(new { message = "Password must be between 8 and 128 characters." });
        }

        var roleName = request.Role?.Trim() ?? string.Empty;
        if (!AllowedSelfRegistrationRoles.Contains(roleName))
        {
            return BadRequest(new { message = $"Invalid role. Allowed values: {string.Join(", ", AllowedSelfRegistrationRoles)}." });
        }

        try
        {
            var userId = await _keycloak.CreateUserAsync(new KeycloakCreateUserRequest
            {
                Username = request.Email.Trim().ToLowerInvariant(),
                Email = request.Email.Trim().ToLowerInvariant(),
                FirstName = request.FullName.Trim(),
                Enabled = true,
                EmailVerified = true,
                Credentials = new List<KeycloakCredential>
                {
                    new() { Type = "password", Value = request.Password, Temporary = false }
                }
            }, cancellationToken);

            await _keycloak.AssignRealmRoleAsync(userId, roleName, cancellationToken);

            return Ok(new { userId });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return Conflict(new { message = "An account with this email already exists." });
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new { message = "Identity service is unavailable. Please try again shortly." });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        try
        {
            var token = await _keycloak.PasswordLoginAsync(request.Email.Trim().ToLowerInvariant(), request.Password, cancellationToken);
            return Ok(new
            {
                token = token.AccessToken,
                refreshToken = token.RefreshToken,
                expiresIn = token.ExpiresIn
            });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new { message = "Identity service is unavailable. Please try again shortly." });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required." });
        }

        var baseUrl = _configuration["Keycloak:BaseUrl"] ?? "http://keycloak:8080";
        var realm = _configuration["Keycloak:Realm"] ?? "buildpay";
        var clientId = _configuration["Keycloak:PublicClientId"] ?? "buildpay-web";

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = clientId,
            ["refresh_token"] = request.RefreshToken
        };

        try
        {
            var client = _httpClientFactory.CreateClient("keycloak");
            using var response = await client.PostAsync($"{baseUrl}/realms/{realm}/protocol/openid-connect/token", new FormUrlEncodedContent(form), cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return Unauthorized(new { message = "Refresh token is expired or invalid." });
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: cancellationToken);
            if (json is null)
            {
                return StatusCode(503, new { message = "Identity service is unavailable. Please try again shortly." });
            }

            return Ok(new
            {
                token = json.AccessToken,
                refreshToken = json.RefreshToken,
                expiresIn = json.ExpiresIn
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new { message = "Identity service is unavailable. Please try again shortly." });
        }
    }

    public sealed record RegisterRequest(string FullName, string Email, string Password, string Role);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record RefreshRequest(string RefreshToken);
}
