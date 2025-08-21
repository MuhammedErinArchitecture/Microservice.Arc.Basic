using Microsoft.Extensions.Caching.Memory;

namespace ContentService.Infrastructure.InterService.UserApi;

public interface IAuthorValidator
{
    Task<bool> ExistsAsync(Guid authorId, CancellationToken ct = default);
}

public sealed class AuthorValidator(IUserApiClient client, IMemoryCache cache) : IAuthorValidator
{
    private readonly IUserApiClient _client = client;
    private readonly IMemoryCache _cache = cache;

    public Task<bool> ExistsAsync(Guid authorId, CancellationToken ct = default)
      => _cache.GetOrCreateAsync($"user-exists:{authorId}", async e =>
      {
          e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
          return await _client.ExistsAsync(authorId, ct);
      })!;
}