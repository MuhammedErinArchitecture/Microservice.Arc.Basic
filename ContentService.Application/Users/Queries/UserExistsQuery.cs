using BuildingBlocks.Abstractions.CQRS;

namespace ContentService.Application.Users.Queries;

public sealed record UserExistsQuery(Guid UserId) : IQuery<bool>;
