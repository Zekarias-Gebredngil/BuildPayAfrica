using System.Text.Json.Serialization;

namespace BuildPay.Infrastructure.Keycloak;

public sealed class KeycloakCreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<KeycloakCredential> Credentials { get; set; } = new();
}

public sealed class KeycloakCredential
{
    public string Type { get; set; } = "password";
    public string Value { get; set; } = string.Empty;
    public bool Temporary { get; set; } = false;
}

public sealed class KeycloakRoleRepresentation
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";
}

