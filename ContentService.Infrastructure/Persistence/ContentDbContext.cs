using ContentService.Domain.Contents;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Infrastructure.Persistence;

public sealed class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options) { }

    public DbSet<Content> Contents => Set<Content>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Content>();
        e.ToTable("contents");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.Title).HasColumnName("title").IsRequired();
        e.Property(x => x.Body).HasColumnName("body").IsRequired();
        e.Property(x => x.Slug).HasColumnName("slug").IsRequired();
        e.HasIndex(x => x.Slug).IsUnique();
        e.Property(x => x.Status).HasColumnName("status").HasDefaultValue("Draft");
        e.Property(x => x.AuthorId).HasColumnName("author_id").IsRequired();
        e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        e.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
    }
}
