using Abstractions.Persistence;
using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.Persistence;
using UserService.Application.Users.Queries;
using UserService.Domain.Users;

namespace UserService.Infrastructure.Persistence.Read;

public sealed class GetUsersPagedQueryHandler
  : IQueryHandler<GetUsersPagedQuery, IPagedResult<User>>
{
    private readonly IReadRepository<User, Guid> _read;
    public GetUsersPagedQueryHandler(IReadRepository<User, Guid> read) => _read = read;

    public Task<IPagedResult<User>> Handle(GetUsersPagedQuery q, CancellationToken ct = default)
      => _read.GetPagedAsync(q.Page, q.PageSize, q.Search, null, ct);
}