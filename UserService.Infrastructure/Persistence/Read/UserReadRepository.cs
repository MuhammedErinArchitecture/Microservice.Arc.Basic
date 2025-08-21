using Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence;
using Dapper;
using UserService.Domain.Users;

namespace UserService.Infrastructure.Persistence.Read;

public sealed class UserReadRepository : IReadRepository<User, Guid>
{
    private readonly IReadDbConnectionFactory _f;
    public UserReadRepository(IReadDbConnectionFactory f) => _f = f;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"select id, first_name as FirstName, last_name as LastName,
                                email as Email, role, status, created_at_utc as CreatedAtUtc, updated_at_utc as UpdatedAtUtc
                         from users where id = @id limit 1";
        using var c = await _f.CreateOpenConnectionAsync(ct);
        return await c.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { id }, cancellationToken: ct));
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "select exists (select 1 from users where id = @id)";
        using var c = await _f.CreateOpenConnectionAsync(ct);
        return await c.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { id }, cancellationToken: ct));
    }

    public async Task<IPagedResult<User>> GetPagedAsync(int page, int pageSize, string? search, string? orderBy, CancellationToken ct = default)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 200);
        var where = string.IsNullOrWhiteSpace(search) ? "" : "where (first_name ilike @k or last_name ilike @k or email ilike @k)";
        var sort = string.IsNullOrWhiteSpace(orderBy) ? "order by created_at_utc desc" : "order by created_at_utc desc"; // sade

        var sql = $@"
      select id, first_name as FirstName, last_name as LastName, email as Email, role, status,
             created_at_utc as CreatedAtUtc, updated_at_utc as UpdatedAtUtc
      from users {where} {sort}
      limit @take offset @skip;

      select count(*) from users {where};
    ";

        using var c = await _f.CreateOpenConnectionAsync(ct);
        using var multi = await c.QueryMultipleAsync(new CommandDefinition(sql, new { k = $"%{search}%", take = pageSize, skip = (page - 1) * pageSize }, cancellationToken: ct));
        var items = (await multi.ReadAsync<User>()).ToList();
        var total = await multi.ReadFirstAsync<long>();

        return new PagedResult<User>(page, pageSize, total, items);
    }
}
