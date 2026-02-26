using Contracting.Shared.Common;

namespace Contracting.API.Middleware;

/// <summary>
/// Reads the <c>X-Timezone</c> header from each request and sets
/// <see cref="DateTimeHelper"/>'s per-request timezone accordingly.
///
/// Supported header values:
///   • Windows timezone ID — "Egypt Standard Time", "Arabian Standard Time", "Arab Standard Time"
///   • IANA timezone ID   — "Africa/Cairo", "Asia/Dubai", "Asia/Qatar"
///   • UTC offset         — "+02:00", "+04:00", "+03:00"
///
/// If the header is missing or invalid, the default timezone from appsettings is used.
/// </summary>
public class TimezoneMiddleware
{
    private readonly RequestDelegate _next;
    private const string TimezoneHeader = "X-Timezone";

    public TimezoneMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TimezoneHeader, out var tzValue)
            && !string.IsNullOrWhiteSpace(tzValue))
        {
            DateTimeHelper.SetRequestTimeZone(tzValue!);
        }

        try
        {
            await _next(context);
        }
        finally
        {
            DateTimeHelper.ClearRequestTimeZone();
        }
    }
}
