using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentService.Domain.Contents;

public sealed class Content
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public string Slug { get; private set; }
    public string Status { get; private set; } = "Draft";
    public Guid AuthorId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    private Content() { }

    public Content(Guid id, string title, string body, Guid authorId)
    {
        Id = id == default ? Guid.NewGuid() : id;
        Title = title;
        Body = body;
        AuthorId = authorId;
        Slug = Slugify(title);
    }

    public void Update(string title, string body, string status)
    {
        Title = title;
        Body = body;
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string Slugify(string s)
      => string.Join("-", s.ToLowerInvariant()
                           .Replace("’", "")
                           .Replace("'", "")
                           .Split(' ', StringSplitOptions.RemoveEmptyEntries));
}