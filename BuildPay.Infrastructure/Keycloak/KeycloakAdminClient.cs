using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace BuildPay.Infrastructure.Keycloak;

public sealed class KeycloakAdminClient : IKeycloakAdminClient
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;

    private string? _cachedAdminToken;
    private DateTimeOffset _cachedAdminTokenExpiresAt;

    public KeycloakAdminClient(HttpClient httpClient, IOptions<KeycloakOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> CreateUserAsync(KeycloakCreateUserRequest request, CancellationToken cancellationToken)
    {
        var token = await GetAdminTokenAsync(cancellationToken);

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Location: .../users/{id}
        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrWhiteSpace(location))
        {
            // Fallback: search by username
            return await GetUserIdByUsernameAsync(request.Username, cancellationToken);
        }

        return location.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
    }

    public async Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        var role = await GetRealmRoleAsync(roleName, token, cancellationToken);

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/{userId}/role-mappings/realm");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(new[] { role }), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<KeycloakTokenResponse> PasswordLoginAsync(string username, string password, CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _options.PublicClientId,
            ["username"] = username,
            ["password"] = password
        };

        using var content = new FormUrlEncodedContent(form);
        using var response = await _httpClient.PostAsync(
            $"{_options.BaseUrl}/realms/{_options.Realm}/protocol/openid-connect/token",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<KeycloakTokenResponse>(json) ?? new KeycloakTokenResponse();
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_cachedAdminToken) && _cachedAdminTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return _cachedAdminToken;
        }

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _options.AdminClientId,
            ["username"] = _options.AdminUsername,
            ["password"] = _options.AdminPassword
        };

        using var content = new FormUrlEncodedContent(form);
        using var response = await _httpClient.PostAsync(
            $"{_options.BaseUrl}/realms/{_options.AdminRealm}/protocol/openid-connect/token",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(json) ?? new KeycloakTokenResponse();

        _cachedAdminToken = tokenResponse.AccessToken;
        _cachedAdminTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        return _cachedAdminToken;
    }

    private async Task<string> GetUserIdByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var token = await GetAdminTokenAsync(cancellationToken);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users?username={Uri.EscapeDataString(username)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        var first = doc.RootElement.EnumerateArray().FirstOrDefault();
        var id = first.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException("Could not locate created user.");
        }

        return id!;
    }

    private async Task<KeycloakRoleRepresentation> GetRealmRoleAsync(string roleName, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.BaseUrl}/admin/realms/{_options.Realm}/roles/{Uri.EscapeDataString(roleName)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<KeycloakRoleRepresentation>(json) ?? new KeycloakRoleRepresentation { Name = roleName };
    }
}

