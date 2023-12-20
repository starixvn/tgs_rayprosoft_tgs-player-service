using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tgs.PlayerService.IntegrationTests;

internal class TestHttpHeaderDelegatingHandler(string headerName) : DelegatingHandler
{
    private readonly string _headerName = headerName ?? throw new ArgumentNullException(nameof(headerName));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(_headerName)) 
            return base.SendAsync(request, cancellationToken);
        
        var values = request.Headers.GetValues(_headerName)
            .Select((value, idx) => string.IsNullOrEmpty(value) ? idx.ToString() : value)
            .ToArray();
        
        _ = request.Headers.Remove(_headerName);
        request.Headers.Add(_headerName, values);

        return base.SendAsync(request, cancellationToken);
    }
}