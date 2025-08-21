using Microsoft.EntityFrameworkCore;
using UserService.Domain.Users;

namespace UserService.Infrastructure.Persistence;

public sealed class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.HasPostgresExtension("citext");                 
        var e = model.Entity<User>();
        e.ToTable("users");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.FirstName).HasColumnName("first_name").IsRequired();
        e.Property(x => x.LastName).HasColumnName("last_name").IsRequired();
        e.Property(x => x.Email).HasColumnName("email").HasColumnType("citext").IsRequired();
        e.HasIndex(x => x.Email).IsUnique();
        e.Property(x => x.Role).HasColumnName("role").HasDefaultValue("Author");
        e.Property(x => x.Status).HasColumnName("status").HasDefaultValue("Active");
        e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        e.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
    }
}