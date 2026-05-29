namespace BuildPay.Persistence.Configuration;

public static class RailwayPostgresConnectionString
{
    public static string? FromRailwayEnvironment(Func<string, string?> getEnvironmentVariable)
    {
        var databaseUrl = getEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return FromDatabaseUrl(databaseUrl);
        }

        var host = getEnvironmentVariable("PGHOST");
        var port = getEnvironmentVariable("PGPORT");
        var database = getEnvironmentVariable("PGDATABASE");
        var username = getEnvironmentVariable("PGUSER");
        var password = getEnvironmentVariable("PGPASSWORD");

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(port) ||
            string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    private static string FromDatabaseUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        var connectionString = $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password}";

        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var parameter in query)
        {
            var parts = parameter.Split('=', 2);
            if (parts.Length == 2 && parts[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
            {
                connectionString += $";SSL Mode={ToTitleCase(Uri.UnescapeDataString(parts[1]))};Trust Server Certificate=true";
            }
        }

        return connectionString;
    }

    private static string ToTitleCase(string value)
    {
        return string.IsNullOrEmpty(value)
            ? value
            : char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }
}
