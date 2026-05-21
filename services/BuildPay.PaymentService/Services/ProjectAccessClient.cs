using System.Net;

namespace BuildPay.PaymentService.Services;

public sealed class ProjectAccessClient
{
    private readonly HttpClient _http;

    public ProjectAccessClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<HttpStatusCode> GetAccessStatusAsync(Guid projectId, string? authorizationHeader, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return HttpStatusCode.Unauthorized;
        }

        var req = new HttpRequestMessage(HttpMethod.Get, $"api/projects/{projectId}");
        req.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        var res = await _http.SendAsync(req, ct);
        return res.StatusCode;
    }
}

