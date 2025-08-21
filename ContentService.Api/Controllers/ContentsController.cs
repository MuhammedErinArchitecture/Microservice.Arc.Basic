using BuildingBlocks.Abstractions.CQRS;
using ContentService.Api.Contracts;
using ContentService.Application.Contents.Queries;
using ContentService.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers;

[ApiController]
[Route("api/v1/contents")]
public sealed class ContentsController : ControllerBase
{
    private readonly IContentDomainService _domain;
    private readonly IQueryBus _q;

    public ContentsController(IContentDomainService domain, IQueryBus q)
    { _domain = domain; _q = q; }

    #region Write
    [HttpPost]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Create([FromBody] ContentCreateRequest req, CancellationToken ct)
    {
        var entity = await _domain.CreateAsync(req.Title, req.Body, req.AuthorId, ct);
        return Results.Created($"/api/v1/contents/{entity.Id}", entity.ToResponse());
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> Update([FromRoute] Guid id, [FromBody] ContentUpdateRequest req, CancellationToken ct)
    {
        var e = await _domain.UpdateAsync(id, req.Title, req.Body, req.Status, ct);
        return e is null ? Results.NotFound() : Results.Ok(e.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var ok = await _domain.DeleteAsync(id, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }
    #endregion

    #region Read
    [HttpGet("{id:guid}", Name = "GetContentById")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var entity = await _q.Send(new GetContentByIdQuery(id), ct);
        return entity is null ? Results.NotFound() : Results.Ok(entity.ToResponse());
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ContentResponse>), StatusCodes.Status200OK)]
    public async Task<IResult> GetPaged(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] string? search = null,
      [FromQuery] string? orderBy = null,
      CancellationToken ct = default)
    {
        var result = await _q.Send(new GetContentsPagedQuery(page, pageSize, search, orderBy), ct);
        var items = result.Items.Select(x => x.ToResponse()).ToList();
        return Results.Ok(new PagedResponse<ContentResponse>(result.Page, result.PageSize, result.TotalCount, items));
    }
    #endregion
}
