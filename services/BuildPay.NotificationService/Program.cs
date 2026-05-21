using BuildPay.NotificationService;
using BuildPay.ServiceDefaults;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddBuildPayServiceDefaults(serviceName: "BuildPay.NotificationService");

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDb")));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<IdentityContractorVerifiedConsumer>();
    x.AddConsumer<IdentityContractorRejectedConsumer>();
    x.AddConsumer<ProjectMilestoneCompletedConsumer>();
    x.AddConsumer<PaymentMilestoneReleasedConsumer>();
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<DeliveryUpdatedConsumer>();
    x.AddConsumer<DeliveryCompletedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"));
        NotificationBusTopology.Configure(cfg);
        cfg.ConfigureEndpoints(ctx);
    });
});

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
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseBuildPayServiceDefaults();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

