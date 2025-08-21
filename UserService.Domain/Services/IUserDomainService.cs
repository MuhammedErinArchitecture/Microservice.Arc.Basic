using UserService.Domain.Users;

namespace UserService.Domain.Services;

public interface IUserDomainService
{
    Task<User> CreateAsync(string firstName, string lastName, string email, string? role, CancellationToken ct = default);
    Task<User?> UpdateAsync(Guid id, string firstName, string lastName, string role, string status, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
