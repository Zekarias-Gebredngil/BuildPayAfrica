FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# ── Copy solution + ALL .csproj files (layer-cache for restore) ───────────────
COPY BuildPay.sln ./

# Original monolith projects
COPY BuildPay.API/BuildPay.API.csproj                                   BuildPay.API/
COPY BuildPay.Application/BuildPay.Application.csproj                   BuildPay.Application/
COPY BuildPay.Domain/BuildPay.Domain.csproj                             BuildPay.Domain/
COPY BuildPay.Infrastructure/BuildPay.Infrastructure.csproj             BuildPay.Infrastructure/
COPY BuildPay.Persistence/BuildPay.Persistence.csproj                   BuildPay.Persistence/
COPY BuildPay.Application.Tests/BuildPay.Application.Tests.csproj       BuildPay.Application.Tests/

# Shared libraries
COPY shared/BuildPay.Contracts/BuildPay.Contracts.csproj                shared/BuildPay.Contracts/
COPY shared/BuildPay.ServiceDefaults/BuildPay.ServiceDefaults.csproj    shared/BuildPay.ServiceDefaults/

# API Gateway
COPY BuildPay.Gateway/BuildPay.Gateway.csproj                           BuildPay.Gateway/

# Microservices
COPY services/BuildPay.IdentityService/BuildPay.IdentityService.csproj         services/BuildPay.IdentityService/
COPY services/BuildPay.ProjectService/BuildPay.ProjectService.csproj           services/BuildPay.ProjectService/
COPY services/BuildPay.PaymentService/BuildPay.PaymentService.csproj           services/BuildPay.PaymentService/
COPY services/BuildPay.MarketplaceService/BuildPay.MarketplaceService.csproj   services/BuildPay.MarketplaceService/
COPY services/BuildPay.DeliveryService/BuildPay.DeliveryService.csproj         services/BuildPay.DeliveryService/
COPY services/BuildPay.NotificationService/BuildPay.NotificationService.csproj services/BuildPay.NotificationService/
COPY services/BuildPay.UserProfileService/BuildPay.UserProfileService.csproj   services/BuildPay.UserProfileService/

# ── Restore (cached until any .csproj changes) ────────────────────────────────
RUN dotnet restore ./BuildPay.sln

# ── Copy all source and publish ───────────────────────────────────────────────
COPY . .
RUN dotnet publish BuildPay.API/BuildPay.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BuildPay.API.dll"]
