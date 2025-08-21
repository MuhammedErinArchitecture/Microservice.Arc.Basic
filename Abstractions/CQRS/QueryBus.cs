using BuildingBlocks.Abstractions.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace ContentService.Application.CQRS;

public sealed class QueryBus : IQueryBus
{
    private readonly IServiceProvider _sp;
    public QueryBus(IServiceProvider sp) => _sp = sp;

    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = _sp.GetRequiredService(handlerType);
        return (Task<TResponse>)handler.Handle((dynamic)query, ct);
    }
}
