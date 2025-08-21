using ContentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ContentService.Infrastructure.Persistence.Design;

public sealed class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    public ContentDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ContentService.Api");
        var config = new ConfigurationBuilder()
          .SetBasePath(basePath)
          .AddJsonFile("appsettings.json", optional: true)
          .AddJsonFile($"appsettings.{env}.json", optional: true)
          .AddEnvironmentVariables()
          .Build();

        var conn = config.GetConnectionString("WriteDb")
                   ?? "Host=localhost;Port=5434;Database=contents;Username=app;Password=app";

        var options = new DbContextOptionsBuilder<ContentDbContext>()
          .UseNpgsql(conn, npg => npg.EnableRetryOnFailure())
          .Options;

        return new ContentDbContext(options);
    }
}
