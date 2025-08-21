using BuildingBlocks.Abstractions.Persistence;
using ContentService.Domain.Contents;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Infrastructure.Persistence.Write;

public sealed class ContentWriteRepository : IWriteRepository<Content, Guid>
{
    private readonly ContentDbContext _db;
    public IUnitOfWork UnitOfWork => new EfUnitOfWork(_db);

    public ContentWriteRepository(ContentDbContext db) => _db = db;

    public async Task<Content?> GetByIdAsync(Guid id, CancellationToken ct = default)
      => await _db.Contents.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Content entity, CancellationToken ct = default)
      => await _db.Contents.AddAsync(entity, ct);

    public void Update(Content entity) => _db.Contents.Update(entity);

    public void Remove(Content entity) => _db.Contents.Remove(entity);

    private sealed class EfUnitOfWork : IUnitOfWork
    {
        private readonly ContentDbContext _ctx;
        public EfUnitOfWork(ContentDbContext ctx) => _ctx = ctx;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
          => _ctx.SaveChangesAsync(cancellationToken);
    }
}
