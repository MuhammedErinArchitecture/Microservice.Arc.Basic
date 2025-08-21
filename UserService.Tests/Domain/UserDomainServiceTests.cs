using BuildingBlocks.Abstractions.Persistence;
using FluentAssertions;
using Moq;
using UserService.Application.Users;
using UserService.Domain.Exceptions;
using UserService.Domain.Users;

namespace UserService.Tests.Domain;

public class UserDomainServiceTests
{
    private static (Mock<IUserReadService>, Mock<IWriteRepository<User, Guid>>, Mock<IUnitOfWork>) Mocks()
    {
        var read = new Mock<IUserReadService>();
        var write = new Mock<IWriteRepository<User, Guid>>();
        var uow = new Mock<IUnitOfWork>();
        write.SetupGet(w => w.UnitOfWork).Returns(uow.Object);
        return (read, write, uow);
    }

    [Fact]
    public async Task Create_Should_Throw_409_When_Email_Exists()
    {
        var (read, write, _) = Mocks();
        read.Setup(r => r.EmailExistsAsync("ada@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new UserDomainService(read.Object, write.Object);
        Func<Task> act = () => sut.CreateAsync("Ada", "Lovelace", "ada@example.com", "Author", CancellationToken.None);

        await act.Should().ThrowAsync<EmailConflictException>();
    }

    [Fact]
    public async Task Create_Should_Save_When_Valid()
    {
        var (read, write, uow) = Mocks();
        read.Setup(r => r.EmailExistsAsync("ada@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        write.Setup(w => w.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new UserDomainService(read.Object, write.Object);
        var u = await sut.CreateAsync("Ada", "Lovelace", "ada@example.com", "Author", CancellationToken.None);

        u.Email.Should().Be("ada@example.com");
        write.Verify(w => w.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_Should_Return_Null_When_NotFound()
    {
        var (read, write, uow) = Mocks();
        write.Setup(w => w.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var sut = new UserDomainService(read.Object, write.Object);
        var result = await sut.UpdateAsync(Guid.NewGuid(), "F", "L", "Author", "Active", CancellationToken.None);

        result.Should().BeNull();
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Should_Return_True_When_Removed()
    {
        var (read, write, uow) = Mocks();
        var user = new User(Guid.Empty, "A", "B", "a@b.com");
        write.Setup(w => w.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new UserDomainService(read.Object, write.Object);
        var ok = await sut.DeleteAsync(user.Id, CancellationToken.None);

        ok.Should().BeTrue();
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}