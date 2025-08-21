using Abstractions.Persistence;
using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.Persistence;
using ContentService.Application.Contents;
using ContentService.Application.Contents.Queries;
using ContentService.Application.Users.Queries;
using ContentService.Domain.Contents;
using ContentService.Infrastructure.InterService.UserApi;
using ContentService.Infrastructure.Persistence;
using ContentService.Infrastructure.Persistence.Read;
using ContentService.Infrastructure.Persistence.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace ContentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddContentServiceInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
    {
        var writeCnn = configuration.GetConnectionString("WriteDb")
                     ?? throw new InvalidOperationException("ConnectionStrings:WriteDb missing");

        services.AddDbContext<ContentDbContext>(opt =>
        {
            opt.UseNpgsql(writeCnn, npg => npg.EnableRetryOnFailure());
            opt.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        });

        services.AddScoped<IWriteRepository<Content, Guid>, ContentWriteRepository>();
        services.AddScoped<IReadDbConnectionFactory, ReadDbConnectionFactory>();
        services.AddScoped<IReadRepository<Content, Guid>, ContentReadRepository>();

        var timeoutSec = configuration.GetValue<int?>("Http:TimeoutSeconds") ?? 3;
        var retryCount = configuration.GetValue<int?>("Http:RetryCount") ?? 2;
        var bulkheadMax = configuration.GetValue<int?>("Http:BulkheadMaxParallel") ?? 50;

        services.AddHttpClient("user-api", (sp, c) =>
        {
            var baseAddress = configuration["UserApi:BaseAddress"]
              ?? throw new InvalidOperationException("UserApi:BaseAddress missing");
            c.BaseAddress = new Uri(baseAddress);
            c.Timeout = TimeSpan.FromSeconds(timeoutSec);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        })
        .AddHttpMessageHandler(() => new BulkheadDelegatingHandler(bulkheadMax))        
        .AddPolicyHandler(Policy<HttpResponseMessage>
          .Handle<HttpRequestException>()
          .OrResult(r => (int)r.StatusCode >= 500)
          .WaitAndRetryAsync(retryCount, i => TimeSpan.FromMilliseconds(200 * i)));

        services.AddScoped<IQueryHandler<UserExistsQuery, bool>, UserExistsQueryHandler>();
        services.AddScoped<IQueryHandler<GetContentByIdQuery, Content?>, GetContentByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetContentsPagedQuery, IPagedResult<Content>>, GetContentsPagedQueryHandler>();

        services.AddMemoryCache();
        services.AddScoped<IAuthorValidator, AuthorValidator>();
        services.AddScoped<IContentReadService, ContentReadService>();

        return services;
    }
}
