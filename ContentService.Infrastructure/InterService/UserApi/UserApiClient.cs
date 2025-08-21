using System.Net;

namespace ContentService.Infrastructure.InterService.UserApi;

public sealed class UserApiClient(HttpClient http) : IUserApiClient
{
    private readonly HttpClient _http = http;

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default)
    {
        var res = await _http.GetAsync($"/api/v1/users/{userId}", ct);
        if (res.StatusCode == HttpStatusCode.NotFound) return false;
        res.EnsureSuccessStatusCode();
        return true;
    }
}