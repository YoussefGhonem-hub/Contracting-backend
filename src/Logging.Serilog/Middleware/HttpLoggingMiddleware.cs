using Logging.Serilog.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Logging.Serilog.Middleware;

/// <summary>
/// ASP.NET Core middleware that enriches logs with request-scoped context and,
/// based on configuration, logs request/response details (with redaction and limits)
/// and unhandled exceptions in a structured, Serilog-friendly way.
/// </summary>
public sealed class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestLoggingOptions _opt;

    /// <summary>
    /// Creates the middleware with the next delegate and strongly-typed request logging options.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="opt">Options bound from configuration section "RequestLogging".</param>
    public HttpLoggingMiddleware(RequestDelegate next, IOptions<RequestLoggingOptions> opt)
    {
        _next = next;
        _opt = opt.Value;
    }

    /// <summary>
    /// Middleware entry point. Enriches LogContext with request-scoped properties,
    /// optionally captures request/response bodies for whitelisted routes, writes a summary log,
    /// and logs unhandled exceptions according to options.
    /// </summary>
    /// <param name="ctx">Current HTTP context.</param>
    public async Task Invoke(HttpContext ctx)
    {
        var correlationId = EnsureCorrelationId(ctx, _opt.CorrelationIdHeader);
        var userId = GetUserId(ctx.User) ?? "anonymous";
        var tenantId = ctx.Request.Headers["X-Tenant-Id"].ToString();
        var method = ctx.Request.Method;
        var path = ctx.Request.Path.Value ?? "";

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("TenantId", string.IsNullOrWhiteSpace(tenantId) ? null : tenantId))
        using (LogContext.PushProperty("Method", method))
        using (LogContext.PushProperty("Path", path))
        {
            // Evaluate path scope once
            bool isWhitelisted = IsWhitelisted(ctx);

            // Summary logging:
            // - If LogAllRequests = true => log summaries for ALL requests.
            // - Else => log summaries only for whitelisted paths.
            bool shouldLogSummary = _opt.LogAllRequests || isWhitelisted;

            // BODY logging rules you requested:
            // - If LogAllRequests = true AND EnableBodyLoggingForExceptions = true => log bodies for ALL requests (ignore PathGlobs).
            // - If LogAllRequests = false AND path is whitelisted AND EnableBodyLoggingForExceptions = true => log bodies.
            // Back-compat: still honor EnableBodyLoggingForRequests/EnableBodyLogging for whitelisted routes.
            bool captureBodiesAll = _opt.LogAllRequests && _opt.EnableBodyLoggingForExceptions;
            bool captureBodiesWhitelistWithExceptionsFlag = !_opt.LogAllRequests && isWhitelisted && _opt.EnableBodyLoggingForExceptions;
            bool captureBodiesRequestsFlag = isWhitelisted && (_opt.EnableBodyLoggingForRequests);

            bool shouldCaptureRequestBody =
                IsTextBased(ctx.Request.ContentType) &&
                (captureBodiesAll || captureBodiesWhitelistWithExceptionsFlag || captureBodiesRequestsFlag);

            // Capture request body if needed
            string requestBody = null;
            if (shouldCaptureRequestBody)
            {
                ctx.Request.EnableBuffering();
                using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                var raw = await reader.ReadToEndAsync();
                ctx.Request.Body.Position = 0;
                requestBody = RedactAndTruncate(raw);
            }

            // Swap response body to capture it
            var original = ctx.Response.Body;
            await using var mem = new MemoryStream();
            ctx.Response.Body = mem;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                sw.Stop();

                // Exception logging controlled by LogExceptions
                if (_opt.LogExceptions)
                {
                    var elog = Log.ForContext("QueryString", ctx.Request.QueryString.Value)
                                  .ForContext("StatusCode", 500)
                                  .ForContext("ElapsedMs", sw.ElapsedMilliseconds)
                                  .ForContext("Whitelisted", isWhitelisted);

                    // Include request body on exception when:
                    // - LogAllRequests=true AND EnableBodyLoggingForExceptions=true (all paths), OR
                    // - path is whitelisted AND EnableBodyLoggingForExceptions=true
                    if (_opt.EnableBodyLoggingForExceptions && (captureBodiesAll || isWhitelisted) && requestBody is not null)
                        elog = elog.ForContext("RequestBody", requestBody, destructureObjects: false);

                    elog.Error(ex, "Unhandled exception");
                }

                // RFC7807 response
                if (!ctx.Response.HasStarted)
                {
                    ctx.Response.Clear();
                    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    ctx.Response.ContentType = "application/problem+json; charset=utf-8";

                    var problem = new
                    {
                        type = "about:blank",
                        title = "An unexpected error occurred.",
                        status = 500,
                        detail = "Please contact support with the correlation id.",
                        traceId = ctx.TraceIdentifier,
                        correlationId
                    };

                    await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
                }

                mem.Seek(0, SeekOrigin.Begin);
                await mem.CopyToAsync(original);
                ctx.Response.Body = original;
                return;
            }
            finally { sw.Stop(); }

            // Normal response path: capture response body if we decided to log request bodies (same decision applies), and content-type is text.
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);
            var respRaw = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);

            string responseBody = null;
            if (IsTextBased(ctx.Response.ContentType) && (captureBodiesAll || captureBodiesWhitelistWithExceptionsFlag || captureBodiesRequestsFlag))
                responseBody = RedactAndTruncate(respRaw);

            // Write summary (per shouldLogSummary)
            if (shouldLogSummary)
            {
                var log = Log.ForContext("QueryString", ctx.Request.QueryString.Value)
                             .ForContext("StatusCode", ctx.Response.StatusCode)
                             .ForContext("ElapsedMs", sw.ElapsedMilliseconds)
                             .ForContext("Whitelisted", isWhitelisted);

                // Include bodies if we captured them
                if (requestBody is not null) log = log.ForContext("RequestBody", requestBody, false);
                if (responseBody is not null) log = log.ForContext("ResponseBody", responseBody, false);

                log.Information("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                                method, path, ctx.Response.StatusCode, sw.ElapsedMilliseconds);
            }

            await mem.CopyToAsync(original);
            ctx.Response.Body = original;
        }
    }

    #region helpers
    /// <summary>
    /// Returns true if the given content type is text-based and thus safe to log.
    /// Uses <see cref="RequestLoggingOptions.TextContentTypes"/> as allowed prefixes (e.g., "application/json", "text/").
    /// </summary>
    private bool IsTextBased(string contentType)
    {
        if (contentType == null) return false;
        foreach (var t in _opt.TextContentTypes)
            if (contentType.StartsWith(t, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    /// <summary>
    /// Determines whether the request path is permitted for detailed logging.
    /// Applies a deny-list first (prefix match), then checks glob-style allow-list in <see cref="RequestLoggingOptions.PathGlobs"/>.
    /// </summary>
    private bool IsWhitelisted(HttpContext ctx)
    {
        var path = ctx.Request.Path.Value ?? string.Empty;

        foreach (var d in _opt.DenyPathsForRequests)
            if (path.StartsWith(d, StringComparison.OrdinalIgnoreCase))
                return false;

        if (_opt.PathGlobsForRequests.Length == 0) return false;

        foreach (var glob in _opt.PathGlobsForRequests)
            if (GlobMatch(path, glob)) return true;

        return false;
    }

    /// <summary>
    /// Simple glob matcher supporting '*' wildcards.
    /// </summary>
    /// <param name="text">The text to test (e.g., a URL path).</param>
    /// <param name="pattern">The glob pattern (e.g., "/api/*/items").</param>
    private static bool GlobMatch(string text, string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return text.Length == 0;
        var parts = pattern.Split('*');
        if (parts.Length == 1)
            return string.Equals(text, pattern, StringComparison.OrdinalIgnoreCase);

        if (!text.StartsWith(parts[0], StringComparison.OrdinalIgnoreCase)) return false;

        int index = parts[0].Length;
        for (int i = 1; i < parts.Length; i++)
        {
            var part = parts[i];
            if (i == parts.Length - 1)
                return text.EndsWith(part, StringComparison.OrdinalIgnoreCase);
            var nextIndex = text.IndexOf(part, index, StringComparison.OrdinalIgnoreCase);
            if (nextIndex < 0) return false;
            index = nextIndex + part.Length;
        }
        return true;
    }

    /// <summary>
    /// تقصّر النص لو طويل جدًا
    /// Redacts configured sensitive keys (JSON) and truncates the payload to the configured size limit.
    /// Falls back to regex redaction for non-JSON content.
    /// </summary>
    private string RedactAndTruncate(string raw)
    {
        string truncated = TruncateUtf8(raw, _opt.BodySizeLimitBytes);

        try
        {
            using var doc = JsonDocument.Parse(truncated);
            var redacted = RedactElement(doc.RootElement);
            return JsonSerializer.Serialize(redacted);
        }
        catch
        {
            // Non-JSON: best-effort regex redaction of configured keys.
            var result = truncated;
            foreach (var key in _opt.SensitiveKeys)
            {
                result = System.Text.RegularExpressions.Regex.Replace(
                    result,
                    "(" + System.Text.RegularExpressions.Regex.Escape(key) + @"(""?\s*[:=]\s*""?))(.*?)((""|\r|\n|,|;))",
                    "$1***REDACTED***$4",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            return result;
        }
    }

    /// <summary>
    /// Truncates a string to at most <paramref name="maxBytes"/> UTF-8 bytes, appending a marker when truncated.
    /// </summary>
    private static string TruncateUtf8(string s, int maxBytes)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        if (bytes.Length <= maxBytes) return s;
        return Encoding.UTF8.GetString(bytes, 0, maxBytes) + "...(truncated)";
    }

    /// <summary>
    /// SensitiveKeys : Recursively redacts configured sensitive keys in a JSON element tree.
    /// Objects and arrays are traversed; scalars are preserved as-is.
    /// </summary>
    private object RedactElement(JsonElement e)
    {
        switch (e.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object>();
                foreach (var p in e.EnumerateObject())
                {
                    bool sensitive = false;
                    foreach (var k in _opt.SensitiveKeys)
                        if (string.Equals(k, p.Name, StringComparison.OrdinalIgnoreCase)) { sensitive = true; break; }

                    dict[p.Name] = sensitive ? "***REDACTED***" : RedactElement(p.Value);
                }
                return dict;

            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var i in e.EnumerateArray())
                    list.Add(RedactElement(i));
                return list;

            default:
                return e.Deserialize<object>();
        }
    }

    /// <summary>
    /// Ensures a correlation id exists for the request:
    /// uses the incoming header when present; otherwise falls back to the ASP.NET trace identifier.
    /// Writes the value to <see cref="HttpContext.Items"/> and response headers for downstream use.
    /// </summary>
    /// <param name="ctx">Current HTTP context.</param>
    /// <param name="headerName">Header name to read/write (e.g., "X-Correlation-Id").</param>
    private static string EnsureCorrelationId(HttpContext ctx, string headerName)
    {
        var cid = ctx.Request.Headers[headerName].ToString();
        if (string.IsNullOrWhiteSpace(cid))
            cid = ctx.TraceIdentifier;

        ctx.Items["CorrelationId"] = cid;
        ctx.Response.Headers[headerName] = cid;
        return cid;
    }

    /// <summary>
    /// Extracts a stable user identifier for logging (NameIdentifier, "sub", or Identity.Name).
    /// Returns null when unauthenticated.
    /// </summary>
    private static string GetUserId(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return null;
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? user.FindFirst("sub")?.Value
               ?? user.Identity.Name;
    }
    #endregion

}
