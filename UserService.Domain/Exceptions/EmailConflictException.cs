namespace UserService.Domain.Exceptions;

public sealed class EmailConflictException : Exception
{
    public string Email { get; }
    public EmailConflictException(string email)
      : base($"Email '{email}' already exists.") => Email = email;
}
