using Logging.Serilog.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Logging.Serilog.Middleware;

/// <summary>
/// Outgoing HTTP message handler that forwards the current request's CorrelationId
/// to downstream services via a configurable header (defaults to "X-Correlation-Id").
/// </summary>
/// <remarks>
/// Works together with <see cref="HttpLoggingMiddleware"/>:
/// - The middleware places the correlation id in HttpContext.Items["CorrelationId"].
/// - This handler reads that value and adds it as a header on outbound HttpClient requests,
///   enabling end-to-end traceability across services.
/// </remarks>
public sealed class CorrelationIdHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;
    private readonly string _header;

    /// <summary>
    /// Creates a new handler.
    /// </summary>
    /// <param name="http">Accessor for the current HttpContext (to read the correlation id).</param>
    /// <param name="options">Request logging options; uses <see cref="RequestLoggingOptions.CorrelationIdHeader"/> for the header name.</param>
    public CorrelationIdHandler(IHttpContextAccessor http, IOptions<RequestLoggingOptions> options)
    {
        _http = http;
        _header = options.Value.CorrelationIdHeader ?? "X-Correlation-Id";
    }

    /// <summary>
    /// Adds the correlation id header to the outgoing request when available and not already present,
    /// then delegates to the next handler.
    /// </summary>
    /// <param name="request">Outgoing HTTP request.</param>
    /// <param name="ct">Cancellation token.</param>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var cid = _http.HttpContext?.Items["CorrelationId"]?.ToString();
        if (!string.IsNullOrEmpty(cid) && !request.Headers.Contains(_header))
            request.Headers.Add(_header, cid);

        return base.SendAsync(request, ct);
    }
}
