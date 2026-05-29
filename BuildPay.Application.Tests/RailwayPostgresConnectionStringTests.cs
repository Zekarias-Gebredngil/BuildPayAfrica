using BuildPay.Persistence.Configuration;
using Xunit;

namespace BuildPay.Application.Tests;

public sealed class RailwayPostgresConnectionStringTests
{
    [Fact]
    public void FromRailwayEnvironment_uses_database_url_when_present()
    {
        var variables = new Dictionary<string, string?>
        {
            ["DATABASE_URL"] = "postgresql://railway:p%40ss%3Aword@postgres.railway.internal:5432/railway?sslmode=require"
        };

        var connectionString = RailwayPostgresConnectionString.FromRailwayEnvironment(variables.GetValueOrDefault);

        Assert.Equal(
            "Host=postgres.railway.internal;Port=5432;Database=railway;Username=railway;Password=p@ss:word;SSL Mode=Require;Trust Server Certificate=true",
            connectionString);
    }

    [Fact]
    public void FromRailwayEnvironment_builds_connection_string_from_pg_variables()
    {
        var variables = new Dictionary<string, string?>
        {
            ["PGHOST"] = "postgres.railway.internal",
            ["PGPORT"] = "5432",
            ["PGDATABASE"] = "railway",
            ["PGUSER"] = "postgres",
            ["PGPASSWORD"] = "secret"
        };

        var connectionString = RailwayPostgresConnectionString.FromRailwayEnvironment(variables.GetValueOrDefault);

        Assert.Equal(
            "Host=postgres.railway.internal;Port=5432;Database=railway;Username=postgres;Password=secret",
            connectionString);
    }
}
