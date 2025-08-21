using ContentService.Domain.Contents;

namespace ContentService.Domain.Services;

public interface IContentDomainService
{
    Task<Content> CreateAsync(string title, string body, Guid authorId, CancellationToken ct = default);
    Task<Content?> UpdateAsync(Guid id, string title, string body, string status, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
