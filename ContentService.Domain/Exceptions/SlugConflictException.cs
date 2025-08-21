namespace ContentService.Domain.Exceptions;

public sealed class SlugConflictException : Exception
{
    public string Slug { get; }
    public SlugConflictException(string slug)
      : base($"Slug '{slug}' already exists.") => Slug = slug;
}