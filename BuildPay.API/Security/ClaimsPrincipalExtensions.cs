using System.Security.Claims;

namespace BuildPay.API.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(sub))
        {
            throw new InvalidOperationException("Missing 'sub' claim.");
        }

        if (!Guid.TryParse(sub, out var userId))
        {
            throw new InvalidOperationException("Invalid 'sub' claim format (expected GUID).");
        }

        return userId;
    }

    public static string? GetEmail(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");

    public static string? GetFullName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue("name") ??
        principal.FindFirstValue("given_name") ??
        principal.FindFirstValue(ClaimTypes.Name);

    public static string? GetPhoneNumber(this ClaimsPrincipal principal) => principal.FindFirstValue("phone_number");
}

