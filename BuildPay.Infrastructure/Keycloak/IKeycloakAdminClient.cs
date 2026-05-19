using System.Threading;
using System.Threading.Tasks;

namespace BuildPay.Infrastructure.Keycloak;

public interface IKeycloakAdminClient
{
    Task<string> CreateUserAsync(KeycloakCreateUserRequest request, CancellationToken cancellationToken);
    Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken);
    Task<KeycloakTokenResponse> PasswordLoginAsync(string username, string password, CancellationToken cancellationToken);
}

