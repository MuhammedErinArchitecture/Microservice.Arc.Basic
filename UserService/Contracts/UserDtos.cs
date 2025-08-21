namespace UserService.Api.Contracts;

public sealed record UserCreateRequest(string FirstName, string LastName, string Email, string? Role);
public sealed record UserUpdateRequest(string FirstName, string LastName, string Role, string Status);
public sealed record UserResponse(Guid Id, string FirstName, string LastName, string Email, string Role, string Status, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
public sealed record PagedResponse<T>(int Page, int PageSize, long TotalCount, IReadOnlyList<T> Items);
