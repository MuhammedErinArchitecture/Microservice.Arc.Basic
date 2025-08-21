using ContentService.Domain.Contents;

namespace ContentService.Api.Contracts;

internal static class ContentMappings
{
    public static ContentResponse ToResponse(this Content e) =>
      new(
        e.Id, e.Title, e.Body, e.Slug, e.Status, e.AuthorId,
        e.CreatedAtUtc, e.UpdatedAtUtc
      );
}