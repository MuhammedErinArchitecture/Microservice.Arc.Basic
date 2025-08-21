namespace UserService.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Role { get; private set; } = "Author";
    public string Status { get; private set; } = "Active";
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    private User() { }
    public User(Guid id, string first, string last, string email)
    {
        Id = id == default ? Guid.NewGuid() : id;
        FirstName = first; LastName = last; Email = email.ToLowerInvariant();
    }
    public void Update(string first, string last, string role, string status)
    {
        FirstName = first; LastName = last; Role = role; Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}