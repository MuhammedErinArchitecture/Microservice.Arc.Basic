using Abstractions.Persistence;
using BuildingBlocks.Abstractions.CQRS;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using UserService.Api.Contracts;
using UserService.Api.Controllers;
using UserService.Application.Users.Queries;
using UserService.Domain.Services;
using UserService.Domain.Users;

namespace UserService.Tests.Controllers;

public class UsersControllerTests
{
    private static User MakeUser(Guid? id = null) =>
        new User(id ?? Guid.NewGuid(), "Ada", "Lovelace", "ada@example.com");

    [Fact]
    public async Task Create_Should_Return_201_When_Success()
    {
        var domain = new Mock<IUserDomainService>();
        var bus = new Mock<IQueryBus>();
        var u = MakeUser();

        domain.Setup(s => s.CreateAsync("Ada", "Lovelace", "ada@example.com", "Author", It.IsAny<CancellationToken>()))
              .ReturnsAsync(u);

        var sut = new UsersController(domain.Object, bus.Object);
        var req = new UserCreateRequest("Ada", "Lovelace", "ada@example.com", "Author");

        var result = await sut.Create(req, CancellationToken.None);
        (result as IStatusCodeHttpResult)!.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Update_Should_Return_404_When_NotFound()
    {
        var domain = new Mock<IUserDomainService>();
        var bus = new Mock<IQueryBus>();
        domain.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var sut = new UsersController(domain.Object, bus.Object);
        var result = await sut.Update(Guid.NewGuid(), new UserUpdateRequest("F", "L", "Author", "Active"), CancellationToken.None);

        (result as IStatusCodeHttpResult)!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Delete_Should_Return_204_When_Success()
    {
        var domain = new Mock<IUserDomainService>();
        var bus = new Mock<IQueryBus>();
        domain.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new UsersController(domain.Object, bus.Object);
        var result = await sut.Delete(Guid.NewGuid(), CancellationToken.None);

        (result as IStatusCodeHttpResult)!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task GetById_Should_Return_200_When_Found()
    {
        var domain = new Mock<IUserDomainService>();
        var bus = new Mock<IQueryBus>();
        var u = MakeUser();

        bus.Setup(b => b.Send(new GetUserByIdQuery(u.Id), It.IsAny<CancellationToken>()))
           .ReturnsAsync(u);

        var sut = new UsersController(domain.Object, bus.Object);
        var result = await sut.GetById(u.Id, CancellationToken.None);

        result.Should().BeOfType<Ok<UserResponse>>();
        var ok = (Ok<UserResponse>)result;
        ok.Value!.Email.Should().Be(u.Email);
    }

    [Fact]
    public async Task GetPaged_Should_Return_200_With_Items()
    {
        var domain = new Mock<IUserDomainService>();
        var bus = new Mock<IQueryBus>();
        var u = MakeUser();

        var paged = new PagedResult<User>(1, 10, 1, new[] { u });
        bus.Setup(b => b.Send(new GetUsersPagedQuery(1, 10, "ada"), It.IsAny<CancellationToken>()))
           .ReturnsAsync(paged);

        var sut = new UsersController(domain.Object, bus.Object);
        var result = await sut.GetPaged(1, 10, "ada", CancellationToken.None);

        result.Should().BeOfType<Ok<PagedResponse<UserResponse>>>();
        var ok = (Ok<PagedResponse<UserResponse>>)result;
        ok.Value!.Items.Single().Email.Should().Be(u.Email);
    }
}
