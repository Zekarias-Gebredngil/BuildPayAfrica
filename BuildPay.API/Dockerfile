FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BuildPay.sln ./
COPY BuildPay.API/BuildPay.API.csproj BuildPay.API/
COPY BuildPay.Application/BuildPay.Application.csproj BuildPay.Application/
COPY BuildPay.Domain/BuildPay.Domain.csproj BuildPay.Domain/
COPY BuildPay.Infrastructure/BuildPay.Infrastructure.csproj BuildPay.Infrastructure/
COPY BuildPay.Persistence/BuildPay.Persistence.csproj BuildPay.Persistence/

RUN dotnet restore ./BuildPay.sln

COPY . .
RUN dotnet publish BuildPay.API/BuildPay.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BuildPay.API.dll"]

