using System.Security.Claims;

namespace BuildPay.ServiceDefaults;

public static class UserContext
{
    public static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    public static bool IsAdmin(ClaimsPrincipal user) => user.IsInRole("Admin");
}

