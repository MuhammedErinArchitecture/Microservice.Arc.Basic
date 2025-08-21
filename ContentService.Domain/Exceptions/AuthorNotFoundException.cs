namespace ContentService.Domain.Exceptions;

public sealed class AuthorNotFoundException : Exception
{
    public Guid AuthorId { get; }
    public AuthorNotFoundException(Guid authorId)
      : base($"Author {authorId} not found.") => AuthorId = authorId;
}