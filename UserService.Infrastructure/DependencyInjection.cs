using Abstractions.Persistence;
using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Users;
using UserService.Application.Users.Queries;
using UserService.Domain.Users;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Persistence.Read;
using UserService.Infrastructure.Persistence.Write;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserServiceInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var write = cfg.GetConnectionString("WriteDb") ?? throw new InvalidOperationException("WriteDb missing");
        services.AddDbContext<UserDbContext>(o => o.UseNpgsql(write, npg => npg.EnableRetryOnFailure()));

        services.AddScoped<IUserReadService, UserReadService>();

        services.AddScoped<IWriteRepository<User, Guid>, UserWriteRepository>();
        services.AddScoped<IReadDbConnectionFactory, ReadDbConnectionFactory>();
        services.AddScoped<IReadRepository<User, Guid>, UserReadRepository>();
        services.AddScoped<IQueryHandler<GetUserByIdQuery, User?>, GetUserByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetUsersPagedQuery, IPagedResult<User>>, GetUsersPagedQueryHandler>();
        return services;
    }
}
