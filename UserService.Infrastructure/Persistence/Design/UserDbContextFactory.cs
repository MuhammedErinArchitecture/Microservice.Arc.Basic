using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UserService.Infrastructure.Persistence.Design;

public sealed class UserDbContextFactory : IDesignTimeDbContextFactory<UserService.Infrastructure.Persistence.UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "UserService.Api"));

        var cfg = new ConfigurationBuilder()
          .AddJsonFile(Path.Combine(apiPath, "appsettings.json"), optional: true)
          .AddJsonFile(Path.Combine(apiPath, $"appsettings.{env}.json"), optional: true)
          .AddEnvironmentVariables()
          .Build();

        var conn = cfg.GetConnectionString("WriteDb") ?? "Host=localhost;Port=5433;Database=users;Username=app;Password=app";
        var opts = new DbContextOptionsBuilder<UserDbContext>()
          .UseNpgsql(conn, npg => npg.EnableRetryOnFailure())
          .Options;

        return new UserDbContext(opts);
    }
}
