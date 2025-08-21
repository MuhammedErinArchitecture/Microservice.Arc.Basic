using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.Persistence;
using ContentService.Application.Contents.Queries;
using ContentService.Domain.Contents;

namespace ContentService.Infrastructure.Persistence.Read;

public sealed class GetContentByIdQueryHandler
  : IQueryHandler<GetContentByIdQuery, Content?>
{
    private readonly IReadRepository<Content, Guid> _read;
    public GetContentByIdQueryHandler(IReadRepository<Content, Guid> read) => _read = read;

    public Task<Content?> Handle(GetContentByIdQuery query, CancellationToken ct = default)
      => _read.GetByIdAsync(query.Id, ct);
}