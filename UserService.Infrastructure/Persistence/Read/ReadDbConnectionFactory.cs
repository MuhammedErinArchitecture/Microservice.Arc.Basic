using System.Data;
using BuildingBlocks.Abstractions.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace UserService.Infrastructure.Persistence.Read;

public sealed class ReadDbConnectionFactory : IReadDbConnectionFactory
{
    private readonly string _conn;
    public ReadDbConnectionFactory(IConfiguration cfg)
      => _conn = cfg.GetConnectionString("ReadDb") ?? throw new InvalidOperationException("ReadDb missing");

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
    {
        var c = new NpgsqlConnection(_conn);
        await c.OpenAsync(ct);
        return c;
    }
}
