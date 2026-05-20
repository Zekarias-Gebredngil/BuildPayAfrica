namespace BuildPay.Infrastructure.Keycloak;

public sealed class KeycloakOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8080";
    public string Realm { get; set; } = "buildpay";

    public string AdminRealm { get; set; } = "master";
    public string AdminClientId { get; set; } = "";
    public string AdminUsername { get; set; } = "";
    public string AdminPassword { get; set; } = "";

    public string PublicClientId { get; set; } = "buildpay-web";
}

