using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ContentService.Infrastructure.InterService.UserApi;

public sealed class BulkheadDelegatingHandler : DelegatingHandler
{
    private readonly SemaphoreSlim _semaphore;

    public BulkheadDelegatingHandler(int maxParallel)
      => _semaphore = new SemaphoreSlim(Math.Max(1, maxParallel));

    protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try { return await base.SendAsync(request, cancellationToken); }
        finally { _semaphore.Release(); }
    }
}