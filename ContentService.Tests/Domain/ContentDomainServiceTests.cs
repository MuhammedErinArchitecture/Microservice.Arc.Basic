using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.Persistence;
using ContentService.Application.Contents;
using ContentService.Application.Users.Queries;
using ContentService.Domain.Contents;
using ContentService.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace ContentService.Tests.Domain;

public class ContentDomainServiceTests
{
    private static Content Make(string title = "T", string body = "Body long enough ...", Guid? authorId = null)
        => new Content(Guid.Empty, title, body, authorId ?? Guid.NewGuid());

    private static (Mock<IQueryBus>, Mock<IContentReadService>, Mock<IWriteRepository<Content, Guid>>, Mock<IUnitOfWork>)
        Mocks()
    {
        var bus = new Mock<IQueryBus>();
        var read = new Mock<IContentReadService>();
        var write = new Mock<IWriteRepository<Content, Guid>>();
        var uow = new Mock<IUnitOfWork>();
        write.SetupGet(w => w.UnitOfWork).Returns(uow.Object);
        return (bus, read, write, uow);
    }

    [Fact]
    public async Task Create_Should_Throw_422_When_Author_Not_Found()
    {
        var (bus, read, write, _) = Mocks();
        bus.Setup(b => b.Send(It.IsAny<UserExistsQuery>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(false);

        var sut = new ContentDomainService(bus.Object, read.Object, write.Object);

        Func<Task> act = () => sut.CreateAsync("T", "B...", Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<AuthorNotFoundException>();
    }

    [Fact]
    public async Task Create_Should_Throw_409_When_Slug_Exists()
    {
        var (bus, read, write, _) = Mocks();
        bus.Setup(b => b.Send(It.IsAny<UserExistsQuery>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(true);
        read.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new ContentDomainService(bus.Object, read.Object, write.Object);

        Func<Task> act = () => sut.CreateAsync("Same Title", "B...", Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<SlugConflictException>();
    }

    [Fact]
    public async Task Create_Should_Save_When_Valid()
    {
        var (bus, read, write, uow) = Mocks();
        bus.Setup(b => b.Send(It.IsAny<UserExistsQuery>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(true);
        read.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        write.Setup(w => w.AddAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        var sut = new ContentDomainService(bus.Object, read.Object, write.Object);
        var entity = await sut.CreateAsync("Unique Title", "B...", Guid.NewGuid(), CancellationToken.None);

        entity.Should().NotBeNull();
        write.Verify(w => w.AddAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_Should_Return_Null_When_NotFound()
    {
        var (bus, read, write, uow) = Mocks();
        write.Setup(w => w.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Content?)null);

        var sut = new ContentDomainService(bus.Object, read.Object, write.Object);
        var result = await sut.UpdateAsync(Guid.NewGuid(), "T2", "B2", "Draft", CancellationToken.None);

        result.Should().BeNull();
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_Should_Save_When_Found()
    {
        var (bus, read, write, uow) = Mocks();
        var entity = Make();
        write.Setup(w => w.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new ContentDomainService(bus.Object, read.Object, write.Object);
        var result = await sut.UpdateAsync(entity.Id, "T2", "B2", "Published", CancellationToken.None);

        result.Should().NotBeNull();
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Should_Return_False_When_NotFound()
    {
        var (bus, read, write, uow) = Mocks();
        write.Setup(w => w.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Content?)null);

        var sut = new ContentDomainService(bus.Object, read.Object, write.Object);
        var ok = await sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        ok.Should().BeFalse();
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Should_Return_True_When_Removed()
    {
        var (bus, read, write, uow) = Mocks();
        var entity = Make();
        write.Setup(w => w.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new ContentDomainService(bus.Object, read.Object, write.Object);
        var ok = await sut.DeleteAsync(entity.Id, CancellationToken.None);

        ok.Should().BeTrue();
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}