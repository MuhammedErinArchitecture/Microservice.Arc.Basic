namespace UserService.Application.Users;

public interface IUserReadService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
