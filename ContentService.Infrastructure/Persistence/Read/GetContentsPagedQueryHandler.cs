using Abstractions.Persistence;
using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.Persistence;
using ContentService.Application.Contents.Queries;
using ContentService.Domain.Contents;

namespace ContentService.Infrastructure.Persistence.Read;

public sealed class GetContentsPagedQueryHandler
  : IQueryHandler<GetContentsPagedQuery, IPagedResult<Content>>
{
    private readonly IReadRepository<Content, Guid> _read;
    public GetContentsPagedQueryHandler(IReadRepository<Content, Guid> read) => _read = read;

    public Task<IPagedResult<Content>> Handle(GetContentsPagedQuery q, CancellationToken ct = default)
      => _read.GetPagedAsync(q.Page, q.PageSize, q.Search, q.OrderBy, ct);
}
