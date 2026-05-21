using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace BuildPay.ServiceDefaults;

public static class ServiceDefaultsExtensions
{
    public static WebApplicationBuilder AddBuildPayServiceDefaults(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Host.UseSerilog((ctx, _, cfg) =>
        {
            cfg
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("service", serviceName)
                .WriteTo.Console(new RenderedCompactJsonFormatter());
        });

        builder.Services.AddHealthChecks();

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                    if (!string.IsNullOrWhiteSpace(endpoint))
                    {
                        options.Endpoint = new Uri(endpoint);
                    }
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter());

        return builder;
    }

    public static WebApplication UseBuildPayServiceDefaults(this WebApplication app, bool mapHealthAndMetrics = true)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();

        if (mapHealthAndMetrics)
        {
            app.MapHealthChecks("/health");
            app.MapPrometheusScrapingEndpoint("/metrics");
        }

        return app;
    }
}
