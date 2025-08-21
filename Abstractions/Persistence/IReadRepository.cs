using Abstractions.Persistence;

namespace BuildingBlocks.Abstractions.Persistence;

public interface IReadRepository<TRead, in TId>
  where TRead : class
{
    Task<TRead?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<bool> ExistsAsync(TId id, CancellationToken ct = default);

    Task<IPagedResult<TRead>> GetPagedAsync(
      int page,
      int pageSize,
      string? search = null,
      string? orderBy = null,
      CancellationToken ct = default);
}

