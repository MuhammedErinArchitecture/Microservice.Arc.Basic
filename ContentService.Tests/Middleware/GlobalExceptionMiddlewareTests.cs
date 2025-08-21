using ContentService.Api.Middleware;
using ContentService.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace ContentService.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private sealed class FakeEnv : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    private static async Task<(int Status, string Title)> InvokeAndReadAsync(Exception ex)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var mw = new GlobalExceptionHandlingMiddleware(_ => throw ex, new FakeEnv());
        await mw.Invoke(ctx);

        ctx.Response.Body.Position = 0;
        var json = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        var doc = JsonDocument.Parse(json);
        var status = doc.RootElement.GetProperty("status").GetInt32();
        var title = doc.RootElement.GetProperty("title").GetString()!;
        return (status, title);
    }

    [Fact]
    public async Task Should_Map_AuthorNotFound_To_422()
    {
        var (status, title) = await InvokeAndReadAsync(new AuthorNotFoundException(Guid.NewGuid()));
        status.Should().Be(422);
        title.Should().Be("Author not found");
    }

    [Fact]
    public async Task Should_Map_SlugConflict_To_409()
    {
        var (status, title) = await InvokeAndReadAsync(new SlugConflictException("slug-1"));
        status.Should().Be(409);
        title.Should().Be("Slug conflict");
    }

    [Fact]
    public async Task Should_Map_DbUniqueViolation_To_409()
    {
        var inner = new FakePgException("23505", "IX_contents_slug");
        var db = new DbUpdateException("unique", inner);

        var (status, title) = await InvokeAndReadAsync(db);
        status.Should().Be(409);
        title.Should().Contain("conflict");
    }

    private sealed class FakePgException : Exception
    {
        public string SqlState { get; }
        public string ConstraintName { get; }
        public FakePgException(string sqlState, string constraint) { SqlState = sqlState; ConstraintName = constraint; }
    }
}