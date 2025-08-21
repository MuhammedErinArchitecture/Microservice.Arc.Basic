using BuildingBlocks.Abstractions.CQRS;
using UserService.Domain.Users;

namespace UserService.Application.Users.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<User?>;
