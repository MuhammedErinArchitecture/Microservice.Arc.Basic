using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.Persistence;
using ContentService.Application.Users.Queries;  
using ContentService.Domain.Contents;
using ContentService.Domain.Exceptions;
using ContentService.Domain.Services;

namespace ContentService.Application.Contents;

public sealed class ContentDomainService : IContentDomainService
{
    private readonly IQueryBus _bus;
    private readonly IContentReadService _read;
    private readonly IWriteRepository<Content, Guid> _write;

    public ContentDomainService(IQueryBus bus, IContentReadService read, IWriteRepository<Content, Guid> write)
    { _bus = bus; _read = read; _write = write; }

    public async Task<Content> CreateAsync(string title, string body, Guid authorId, CancellationToken ct = default)
    {
        var exists = await _bus.Send(new UserExistsQuery(authorId), ct);
        if (!exists) throw new AuthorNotFoundException(authorId); 

        var entity = new Content(Guid.Empty, title, body, authorId);
        if (await _read.SlugExistsAsync(entity.Slug, ct))
            throw new SlugConflictException(entity.Slug); 

        await _write.AddAsync(entity, ct);
        await _write.UnitOfWork.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Content?> UpdateAsync(Guid id, string title, string body, string status, CancellationToken ct = default)
    {
        var e = await _write.GetByIdAsync(id, ct);
        if (e is null) return null;

        e.Update(title, body, status);
        await _write.UnitOfWork.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _write.GetByIdAsync(id, ct);
        if (e is null) return false;

        _write.Remove(e);
        await _write.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
