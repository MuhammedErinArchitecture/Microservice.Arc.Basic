namespace BuildingBlocks.Abstractions.CQRS;

public interface IQuery<TResponse> { }

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken ct = default);
}

public interface IQueryBus
{
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);
}
