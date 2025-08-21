namespace BuildingBlocks.Abstractions.Persistence;

public interface IPagedResult<out T>
{
    int Page { get; }
    int PageSize { get; }
    long TotalCount { get; }
    IReadOnlyList<T> Items { get; }
}

public sealed record PagedResult<T>(
  int Page,
  int PageSize,
  long TotalCount,
  IReadOnlyList<T> Items
) : IPagedResult<T>;
