using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Users;

namespace UserService.Infrastructure.Persistence.Write;

public sealed class UserWriteRepository : IWriteRepository<User, Guid>
{
    private readonly UserDbContext _db;
    public IUnitOfWork UnitOfWork => new EfUoW(_db);
    public UserWriteRepository(UserDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
      => await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(User entity, CancellationToken ct = default)
      => await _db.Users.AddAsync(entity, ct);

    public void Update(User entity) => _db.Users.Update(entity);
    public void Remove(User entity) => _db.Users.Remove(entity);

    private sealed class EfUoW(UserDbContext ctx) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => ctx.SaveChangesAsync(ct);
    }
}
