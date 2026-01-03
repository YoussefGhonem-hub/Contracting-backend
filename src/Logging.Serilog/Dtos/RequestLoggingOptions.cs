namespace Logging.Serilog.Dtos;

/// <summary>
/// Per-service configurable options (bound from "RequestLogging")
/// </summary>
public sealed class RequestLoggingOptions
{
    // Exceptions
    // true: write exception logs from middleware; false: suppress them (other sinks/middleware may still log).
    public bool LogExceptions { get; set; } = true;

    // Request/response logging (metadata vs. bodies)
    // true: write a one-line summary for every request (method, path, status, elapsed).
    // false: only write summaries for whitelisted endpoints (PathGlobs).
    public bool LogAllRequests { get; set; } = false;

    // NEW: body logging controls (split)
    // true: capture and log request/response bodies for normal requests (subject to whitelist and content-type/size checks).
    public bool EnableBodyLoggingForRequests { get; set; } = true;

    // true: capture and log request body on exception path (subject to whitelist and content-type/size checks).
    public bool EnableBodyLoggingForExceptions { get; set; } = true;


    // Detailed logging scope
    public string[] PathGlobsForRequests { get; set; } = System.Array.Empty<string>();
    public string[] DenyPathsForRequests { get; set; } = System.Array.Empty<string>();

    // Body capture safety
    public string[] TextContentTypes { get; set; } = new[] { "application/json", "text/" };
    public int BodySizeLimitBytes { get; set; } = 32 * 1024;

    // Redaction
    public string[] SensitiveKeys { get; set; } = new[] { "password", "token", "authorization", "client_secret" };

    // Correlation
    public string CorrelationIdHeader { get; set; } = "X-Correlation-Id";
}


