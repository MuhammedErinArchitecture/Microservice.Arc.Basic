namespace ContentService.Infrastructure.InterService.UserApi;

public interface IUserApiClient
{
    Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default);
}