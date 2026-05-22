using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace BuildPay.ServiceDefaults;

/// <summary>
/// Converts Keycloak JWT role data (realm_access/resource_access) into standard ClaimTypes.Role claims
/// so ASP.NET Core [Authorize(Roles="...")] works reliably across services.
/// </summary>
public sealed class KeycloakRoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        AddRoleClaimsFromKeycloakRealmAccess(identity, principal);
        AddRoleClaimsFromKeycloakResourceAccess(identity, principal);

        return Task.FromResult(principal);
    }

    private static void AddRoleClaimsFromKeycloakRealmAccess(ClaimsIdentity identity, ClaimsPrincipal principal)
    {
        var realmAccess = principal.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(realmAccess);
            if (!doc.RootElement.TryGetProperty("roles", out var rolesElement) || rolesElement.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var role in rolesElement.EnumerateArray())
            {
                var roleName = role.GetString();
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    continue;
                }

                if (!identity.HasClaim(ClaimTypes.Role, roleName))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }
        catch (JsonException)
        {
            // Ignore malformed tokens
        }
    }

    private static void AddRoleClaimsFromKeycloakResourceAccess(ClaimsIdentity identity, ClaimsPrincipal principal)
    {
        var resourceAccess = principal.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccess))
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(resourceAccess);
            foreach (var clientProperty in doc.RootElement.EnumerateObject())
            {
                if (!clientProperty.Value.TryGetProperty("roles", out var rolesElement) || rolesElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var role in rolesElement.EnumerateArray())
                {
                    var roleName = role.GetString();
                    if (string.IsNullOrWhiteSpace(roleName))
                    {
                        continue;
                    }

                    if (!identity.HasClaim(ClaimTypes.Role, roleName))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Ignore malformed tokens
        }
    }
}

