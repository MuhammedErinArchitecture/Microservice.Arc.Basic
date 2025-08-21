using BuildingBlocks.Abstractions.CQRS;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Contracts;
using UserService.Application.Users.Queries;
using UserService.Domain.Services;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserDomainService _domain;
    private readonly IQueryBus _q;

    public UsersController(IUserDomainService domain, IQueryBus q)
    { _domain = domain; _q = q; }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Create([FromBody] UserCreateRequest req, CancellationToken ct)
    {
        var u = await _domain.CreateAsync(req.FirstName, req.LastName, req.Email, req.Role, ct);
        return Results.Created($"/api/v1/users/{u.Id}",
          new UserResponse(u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.Status, u.CreatedAtUtc, u.UpdatedAtUtc));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> Update(Guid id, [FromBody] UserUpdateRequest req, CancellationToken ct)
    {
        var u = await _domain.UpdateAsync(id, req.FirstName, req.LastName, req.Role, req.Status, ct);
        return u is null
          ? Results.NotFound()
          : Results.Ok(new UserResponse(u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.Status, u.CreatedAtUtc, u.UpdatedAtUtc));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _domain.DeleteAsync(id, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetById(Guid id, CancellationToken ct)
    {
        var u = await _q.Send(new GetUserByIdQuery(id), ct);
        return u is null
          ? Results.NotFound()
          : Results.Ok(new UserResponse(u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.Status, u.CreatedAtUtc, u.UpdatedAtUtc));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var r = await _q.Send(new GetUsersPagedQuery(page, pageSize, search), ct);
        var items = r.Items.Select(u => new UserResponse(u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.Status, u.CreatedAtUtc, u.UpdatedAtUtc)).ToList();
        return Results.Ok(new PagedResponse<UserResponse>(r.Page, r.PageSize, r.TotalCount, items));
    }
}