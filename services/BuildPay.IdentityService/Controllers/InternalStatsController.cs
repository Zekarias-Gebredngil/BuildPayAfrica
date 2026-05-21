using BuildPay.Infrastructure.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BuildPay.IdentityService.Controllers;

[ApiController]
[Route("internal/stats")]
[AllowAnonymous]
public sealed class InternalStatsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KeycloakOptions _options;

    public InternalStatsController(IHttpClientFactory httpClientFactory, IOptions<KeycloakOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    [HttpGet("users")]
    public async Task<ActionResult<object>> TotalUsers(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("keycloak");
            // Reuse admin login path from KeycloakAdminClient via raw request; simplest for scaffold.
            // We call the admin token endpoint with configured admin credentials.
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = _options.AdminClientId,
                ["username"] = _options.AdminUsername,
                ["password"] = _options.AdminPassword
            };

            using var tokenRes = await client.PostAsync(
                $"{_options.BaseUrl}/realms/{_options.AdminRealm}/protocol/openid-connect/token",
                new FormUrlEncodedContent(form),
                ct);

            tokenRes.EnsureSuccessStatusCode();
            var tokenJson = await tokenRes.Content.ReadAsStringAsync(ct);
            var token = JsonSerializer.Deserialize<KeycloakTokenResponse>(tokenJson);
            var accessToken = token?.AccessToken;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Ok(new { count = 0 });
            }

            using var req = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/count");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var res = await client.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();
            var countText = await res.Content.ReadAsStringAsync(ct);
            if (!int.TryParse(countText, out var count))
            {
                count = 0;
            }

            return Ok(new { count });
        }
        catch
        {
            return Ok(new { count = 0 });
        }
    }
}
