using Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence;
using ContentService.Domain.Contents;
using Dapper;

namespace ContentService.Infrastructure.Persistence.Read;

public sealed class ContentReadRepository : IReadRepository<Content, Guid>
{
    private readonly IReadDbConnectionFactory _factory;

    public ContentReadRepository(IReadDbConnectionFactory factory)
      => _factory = factory;

    public async Task<Content?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"select id, title, body, slug, status, author_id as AuthorId,
                                created_at_utc as CreatedAtUtc, updated_at_utc as UpdatedAtUtc
                         from contents where id = @id limit 1";
        using var conn = await _factory.CreateOpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Content>(new CommandDefinition(sql, new { id }, cancellationToken: ct));
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "select exists (select 1 from contents where id = @id)";
        using var conn = await _factory.CreateOpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { id }, cancellationToken: ct));
    }

    public async Task<IPagedResult<Content>> GetPagedAsync(
        int page, int pageSize, string? search = null, string? orderBy = null, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var where = string.IsNullOrWhiteSpace(search)
          ? ""
          : "where (title ilike @k or slug ilike @k)";
        var sort = BuildOrderBy(orderBy);

        var sql = $@"
      select id, title, body, slug, status, author_id as AuthorId, created_at_utc as CreatedAtUtc, updated_at_utc as UpdatedAtUtc
      from contents
      {where}
      {sort}
      limit @take offset @skip;

      select count(*) from contents {where};
    ";

        using var conn = await _factory.CreateOpenConnectionAsync(ct);
        using var multi = await conn.QueryMultipleAsync(new CommandDefinition(sql, new
        {
            k = $"%{search}%",
            take = pageSize,
            skip = (page - 1) * pageSize
        }, cancellationToken: ct));

        var items = (await multi.ReadAsync<Content>()).ToList();
        var total = await multi.ReadFirstAsync<long>();

        return new PagedResult<Content>(page, pageSize, total, items);
    }

    private static string BuildOrderBy(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy)) return "order by created_at_utc desc";
        var key = orderBy.Trim().ToLowerInvariant();

        return key switch
        {
            "title" => "order by title asc",
            "-title" => "order by title desc",
            "slug" => "order by slug asc",
            "-slug" => "order by slug desc",
            "status" => "order by status asc",
            "-status" => "order by status desc",
            "created" or "created_at" or "created_at_utc" => "order by created_at_utc asc",
            "-created" or "-created_at" or "-created_at_utc" => "order by created_at_utc desc",
            _ => "order by created_at_utc desc"
        };
    }
}
