using System.Security.Claims;
using System.Threading.RateLimiting;
using BuildPay.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddBuildPayServiceDefaults(serviceName: "BuildPay.Gateway");

builder.Services.AddOpenApi();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            NameClaimType = "preferred_username"
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("{\"message\":\"Authentication required. Please sign in.\"}");
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode = 403;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("{\"message\":\"You do not have permission to access this resource.\"}");
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? httpContext.User.FindFirstValue("sub")
                         ?? httpContext.User.FindFirstValue("preferred_username")
                         ?? "unknown-user";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"user:{userId}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"ip:{ip}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

builder.Services.AddHttpClient("gateway");

var app = builder.Build();

app.UseBuildPayServiceDefaults(mapHealthAndMetrics: false);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapPrometheusScrapingEndpoint("/metrics");

app.MapGet("/health", async (IConfiguration cfg, IHttpClientFactory httpClientFactory, CancellationToken ct) =>
{
    var downstream = cfg.GetSection("DownstreamHealth").Get<Dictionary<string, string>>() ?? new();
    var client = httpClientFactory.CreateClient("gateway");

    var checks = downstream.Select(async kvp =>
    {
        try
        {
            using var res = await client.GetAsync(kvp.Value, ct);
            return new { service = kvp.Key, ok = res.IsSuccessStatusCode };
        }
        catch
        {
            return new { service = kvp.Key, ok = false };
        }
    });

    var results = await Task.WhenAll(checks);
    var okAll = results.All(r => r.ok);

    return Results.Json(new
    {
        status = okAll ? "Healthy" : "Degraded",
        services = results
    }, statusCode: okAll ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
}).AllowAnonymous();

app.MapGet("/api/admin/overview", async (IConfiguration cfg, IHttpClientFactory httpClientFactory, CancellationToken ct) =>
{
    var endpoints = cfg.GetSection("AdminOverview").Get<Dictionary<string, string>>() ?? new();
    var client = httpClientFactory.CreateClient("gateway");

    async Task<int> ReadCount(string url)
    {
        using var res = await client.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: ct);
        if (doc.ValueKind == System.Text.Json.JsonValueKind.Object &&
            doc.TryGetProperty("count", out var countEl) &&
            countEl.TryGetInt32(out var count))
        {
            return count;
        }

        return 0;
    }

    try
    {
        var users = endpoints.TryGetValue("users_total", out var u) ? await ReadCount(u) : 0;
        var contractorsPending = endpoints.TryGetValue("verifications_pending", out var v) ? await ReadCount(v) : 0;
        var products = endpoints.TryGetValue("products_total", out var p) ? await ReadCount(p) : 0;
        var orders = endpoints.TryGetValue("orders_total", out var o) ? await ReadCount(o) : 0;
        var projects = endpoints.TryGetValue("projects_total", out var pr) ? await ReadCount(pr) : 0;
        var deliveries = endpoints.TryGetValue("deliveries_total", out var d) ? await ReadCount(d) : 0;

        return Results.Json(new { users, contractorsPending, products, orders, projects, deliveries });
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}).RequireAuthorization("Authenticated");

app.MapReverseProxy();

app.Run();
