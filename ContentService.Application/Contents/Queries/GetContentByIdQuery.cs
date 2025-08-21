using BuildingBlocks.Abstractions.CQRS;
using ContentService.Domain.Contents;

namespace ContentService.Application.Contents.Queries;

public sealed record GetContentByIdQuery(Guid Id) : IQuery<Content?>;
