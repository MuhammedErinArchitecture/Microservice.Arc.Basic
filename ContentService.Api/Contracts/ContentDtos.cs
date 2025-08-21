using System;

namespace ContentService.Api.Contracts;

public sealed record ContentCreateRequest(
  string Title,
  string Body,
  Guid AuthorId
);

public sealed record ContentUpdateRequest(
  string Title,
  string Body,
  string Status // Draft|Published|Archived
);

public sealed record ContentResponse(
  Guid Id,
  string Title,
  string Body,
  string Slug,
  string Status,
  Guid AuthorId,
  DateTime CreatedAtUtc,
  DateTime? UpdatedAtUtc
);

public sealed record PagedResponse<T>(
  int Page,
  int PageSize,
  long TotalCount,
  IReadOnlyList<T> Items
);
