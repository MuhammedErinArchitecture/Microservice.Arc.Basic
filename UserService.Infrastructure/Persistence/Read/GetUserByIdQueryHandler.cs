using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.Persistence;
using UserService.Application.Users.Queries;
using UserService.Domain.Users;

namespace UserService.Infrastructure.Persistence.Read;

public sealed class GetUserByIdQueryHandler
  : IQueryHandler<GetUserByIdQuery, User?>
{
    private readonly IReadRepository<User, Guid> _read;
    public GetUserByIdQueryHandler(IReadRepository<User, Guid> read) => _read = read;

    public Task<User?> Handle(GetUserByIdQuery q, CancellationToken ct = default)
      => _read.GetByIdAsync(q.Id, ct);
}