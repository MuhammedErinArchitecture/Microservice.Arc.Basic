using Abstractions.Persistence;
using BuildingBlocks.Abstractions.CQRS;
using ContentService.Api.Contracts;
using ContentService.Api.Controllers;
using ContentService.Application.Contents.Queries;
using ContentService.Domain.Contents;
using ContentService.Domain.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace ContentService.Tests.Controllers;

public class ContentsControllerTests
{
    private static Content MakeContent(Guid? id = null) =>
        new Content(id ?? Guid.NewGuid(), "Title A", "Body long enough ...", Guid.NewGuid());

    [Fact]
    public async Task Create_Should_Return_201_When_Success()
    {
        var domain = new Mock<IContentDomainService>();
        var bus = new Mock<IQueryBus>();
        var entity = MakeContent();

        domain.Setup(s => s.CreateAsync("Title A", "Body long enough ...", entity.AuthorId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(entity);

        var sut = new ContentsController(domain.Object, bus.Object);
        var req = new ContentCreateRequest("Title A", "Body long enough ...", entity.AuthorId);

        var result = await sut.Create(req, CancellationToken.None);
        var http = result as IStatusCodeHttpResult;

        http!.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Update_Should_Return_404_When_NotFound()
    {
        var domain = new Mock<IContentDomainService>();
        var bus = new Mock<IQueryBus>();
        domain.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((Content?)null);

        var sut = new ContentsController(domain.Object, bus.Object);
        var result = await sut.Update(Guid.NewGuid(), new ContentUpdateRequest("T", "B", "Draft"), CancellationToken.None);

        (result as IStatusCodeHttpResult)!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Update_Should_Return_200_When_Success()
    {
        var domain = new Mock<IContentDomainService>();
        var bus = new Mock<IQueryBus>();
        var entity = MakeContent();
        domain.Setup(s => s.UpdateAsync(entity.Id, "T2", "B2", "Published", It.IsAny<CancellationToken>()))
              .ReturnsAsync(entity);

        var sut = new ContentsController(domain.Object, bus.Object);
        var result = await sut.Update(entity.Id, new ContentUpdateRequest("T2", "B2", "Published"), CancellationToken.None);

        (result as IStatusCodeHttpResult)!.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Delete_Should_Return_404_When_NotFound()
    {
        var domain = new Mock<IContentDomainService>();
        var bus = new Mock<IQueryBus>();
        domain.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = new ContentsController(domain.Object, bus.Object);
        var result = await sut.Delete(Guid.NewGuid(), CancellationToken.None);

        (result as IStatusCodeHttpResult)!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Delete_Should_Return_204_When_Success()
    {
        var domain = new Mock<IContentDomainService>();
        var bus = new Mock<IQueryBus>();
        domain.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new ContentsController(domain.Object, bus.Object);
        var result = await sut.Delete(Guid.NewGuid(), CancellationToken.None);

        (result as IStatusCodeHttpResult)!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task GetById_Should_Return_404_When_NotFound()
    {
        var domain = new Mock<IContentDomainService>();
        var bus = new Mock<IQueryBus>();
        bus.Setup(b => b.Send(new GetContentByIdQuery(It.IsAny<Guid>()), It.IsAny<CancellationToken>()))
           .ReturnsAsync((Content?)null);

        var sut = new ContentsController(domain.Object, bus.Object);
        var result = await sut.GetById(Guid.NewGuid(), CancellationToken.None);

        (result as IStatusCodeHttpResult)!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetById_Should_Return_200_With_Body_When_Found()
    {
        var domain = new Mock<IContentDomainService>();
        var bus = new Mock<IQueryBus>();
        var entity = MakeContent();

        bus.Setup(b => b.Send(new GetContentByIdQuery(entity.Id), It.IsAny<CancellationToken>()))
           .ReturnsAsync(entity);

        var sut = new ContentsController(domain.Object, bus.Object);
        var result = await sut.GetById(entity.Id, CancellationToken.None);

        result.Should().BeOfType<Ok<ContentResponse>>();
        var ok = (Ok<ContentResponse>)result;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task GetPaged_Should_Return_200_With_Items()
    {
        var domain = new Mock<IContentDomainService>();
        var bus = new Mock<IQueryBus>();
        var entity = MakeContent();

        var paged = new PagedResult<Content>(
            1, 10, 1, new[] { entity });

        bus.Setup(b => b.Send(new GetContentsPagedQuery(1, 10, "t", null), It.IsAny<CancellationToken>()))
           .ReturnsAsync(paged);

        var sut = new ContentsController(domain.Object, bus.Object);
        var result = await sut.GetPaged(1, 10, "t", null, CancellationToken.None);

        result.Should().BeOfType<Ok<PagedResponse<ContentResponse>>>();
        var ok = (Ok<PagedResponse<ContentResponse>>)result;
        ok.Value!.TotalCount.Should().Be(1);
        ok.Value!.Items.Single().Id.Should().Be(entity.Id);
    }
}
