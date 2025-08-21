using Abstractions.Persistence;
using BuildingBlocks.Abstractions.CQRS;
using ContentService.Domain.Contents;

namespace ContentService.Application.Contents.Queries;

public sealed record GetContentsPagedQuery(int Page, int PageSize, string? Search, string? OrderBy)
  : IQuery<IPagedResult<Content>>;
