using BuildingBlocks.Abstractions.Persistence;
using ContentService.Application.Contents;
using Dapper;

namespace ContentService.Infrastructure.Persistence.Read;

public sealed class ContentReadService : IContentReadService
{
    private readonly IReadDbConnectionFactory _factory;
    public ContentReadService(IReadDbConnectionFactory factory) => _factory = factory;

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
    {
        const string sql = "select exists (select 1 from contents where slug = @slug)";
        using var c = await _factory.CreateOpenConnectionAsync(ct);
        return await c.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { slug }, cancellationToken: ct));
    }
}