namespace BuildingBlocks.Abstractions.Persistence;
public interface IWriteRepository<T, in TId>

  where T : class
{
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    IUnitOfWork UnitOfWork { get; }
}
