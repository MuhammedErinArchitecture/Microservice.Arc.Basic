using BuildingBlocks.Abstractions.CQRS;
using ContentService.Application.Users.Queries;
using System.Net;

namespace ContentService.Infrastructure.InterService.UserApi;

public sealed class UserExistsQueryHandler : IQueryHandler<UserExistsQuery, bool>
{
    private readonly IHttpClientFactory _factory;
    public UserExistsQueryHandler(IHttpClientFactory factory) => _factory = factory;

    public async Task<bool> Handle(UserExistsQuery query, CancellationToken ct = default)
    {
        var http = _factory.CreateClient("user-api");
        var res = await http.GetAsync($"/api/v1/users/{query.UserId}", ct);
        if (res.StatusCode == HttpStatusCode.NotFound) return false;
        res.EnsureSuccessStatusCode();
        return true;
    }
}