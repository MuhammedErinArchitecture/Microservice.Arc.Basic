using BuildingBlocks.Abstractions.Persistence;
using UserService.Application.Users;
using UserService.Domain.Exceptions;
using UserService.Domain.Services;
using UserService.Domain.Users;

namespace UserService.Application.Users;

public sealed class UserDomainService : IUserDomainService
{
    private readonly IUserReadService _read;
    private readonly IWriteRepository<User, Guid> _write;

    public UserDomainService(IUserReadService read, IWriteRepository<User, Guid> write)
    { _read = read; _write = write; }

    public async Task<User> CreateAsync(string firstName, string lastName, string email, string? role, CancellationToken ct = default)
    {
        var normalized = email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));

        if (await _read.EmailExistsAsync(normalized, ct))
            throw new EmailConflictException(normalized);

        var u = new User(Guid.Empty, firstName, lastName, normalized);
        if (!string.IsNullOrWhiteSpace(role))
            u.Update(u.FirstName, u.LastName, role!, u.Status);

        await _write.AddAsync(u, ct);
        await _write.UnitOfWork.SaveChangesAsync(ct);
        return u;
    }

    public async Task<User?> UpdateAsync(Guid id, string firstName, string lastName, string role, string status, CancellationToken ct = default)
    {
        var u = await _write.GetByIdAsync(id, ct);
        if (u is null) return null;
        u.Update(firstName, lastName, role, status);
        await _write.UnitOfWork.SaveChangesAsync(ct);
        return u;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var u = await _write.GetByIdAsync(id, ct);
        if (u is null) return false;
        _write.Remove(u);
        await _write.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
