using BuildingBlocks.Abstractions.Persistence;
using Dapper;
using UserService.Application.Users;

namespace UserService.Infrastructure.Persistence.Read;

public sealed class UserReadService : IUserReadService
{
    private readonly IReadDbConnectionFactory _factory;
    public UserReadService(IReadDbConnectionFactory factory) => _factory = factory;

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        const string sql = "select exists (select 1 from users where email = @email)";
        using var c = await _factory.CreateOpenConnectionAsync(ct);
        return await c.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { email }, cancellationToken: ct));
    }
}