namespace ContentService.Application.Contents;

public interface IContentReadService
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
}
