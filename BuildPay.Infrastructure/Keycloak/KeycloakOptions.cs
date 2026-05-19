namespace BuildPay.Infrastructure.Keycloak;

public sealed class KeycloakOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8080";
    public string Realm { get; set; } = "buildpay";

    public string AdminRealm { get; set; } = "master";
    public string AdminClientId { get; set; } = "admin-cli";
    public string AdminUsername { get; set; } = "admin";
    public string AdminPassword { get; set; } = "admin";

    public string PublicClientId { get; set; } = "buildpay-web";
}

