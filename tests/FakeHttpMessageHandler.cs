using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace app.Tests;

/// <summary>
/// Test double for HttpMessageHandler. Routes each request to a provided delegate
/// and records all (Method, URL) pairs for assertion.
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    public ConcurrentBag<(string Method, string Url)> RequestLog { get; } = new();

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestLog.Add((request.Method.Method, request.RequestUri!.ToString()));
        return Task.FromResult(_handler(request));
    }
}
