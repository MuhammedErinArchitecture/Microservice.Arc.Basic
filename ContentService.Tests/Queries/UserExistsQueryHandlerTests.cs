using ContentService.Application.Users.Queries;
using ContentService.Infrastructure.InterService.UserApi;
using FluentAssertions;
using System.Net;

namespace ContentService.Tests.Queries;

public class UserExistsQueryHandlerTests
{
    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _impl;
        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> impl) => _impl = impl;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_impl(request));
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;
        public StubHttpClientFactory(HttpClient client) => _client = client;
        public HttpClient CreateClient(string name) => _client;
    }

    [Fact]
    public async Task Should_Return_False_On_404()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var factory = new StubHttpClientFactory(client);

        var sut = new UserExistsQueryHandler(factory);
        var ok = await sut.Handle(new UserExistsQuery(Guid.NewGuid()), CancellationToken.None);

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Return_True_On_200()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var factory = new StubHttpClientFactory(client);

        var sut = new UserExistsQueryHandler(factory);
        var ok = await sut.Handle(new UserExistsQuery(Guid.NewGuid()), CancellationToken.None);

        ok.Should().BeTrue();
    }
}