using System.Data;

namespace BuildingBlocks.Abstractions.Persistence;

public interface IReadDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default);
}

