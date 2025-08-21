using Abstractions.Persistence;
using BuildingBlocks.Abstractions.CQRS;
using UserService.Domain.Users;

namespace UserService.Application.Users.Queries;

public sealed record GetUsersPagedQuery(int Page, int PageSize, string? Search)
  : IQuery<IPagedResult<User>>;
