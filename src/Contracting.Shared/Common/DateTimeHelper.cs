namespace Contracting.Shared.Common;

/// <summary>
/// Centralized helper for getting the current date/time in the correct timezone.
/// Supports per-request timezone via the <c>X-Timezone</c> HTTP header.
/// Falls back to the default timezone configured via <see cref="Initialize"/>.
/// 
/// Supported <c>X-Timezone</c> header formats:
///   • Windows timezone ID — e.g. "Egypt Standard Time", "Arabian Standard Time", "Arab Standard Time"
///   • IANA timezone ID   — e.g. "Africa/Cairo", "Asia/Dubai", "Asia/Qatar"  (works on Linux hosts)
///   • UTC offset string  — e.g. "+02:00", "+04:00", "-05:00"
/// </summary>
public static class DateTimeHelper
{
    private static TimeZoneInfo _defaultTimeZone = TimeZoneInfo.Utc;

    /// <summary>
    /// Per-request override set by the timezone middleware.
    /// </summary>
    private static readonly AsyncLocal<TimeZoneInfo?> _requestTimeZone = new();

    /// <summary>
    /// Initialize the default timezone from appsettings (e.g. "Egypt Standard Time").
    /// Called once at application startup.
    /// </summary>
    public static void Initialize(string timeZoneId)
    {
        var resolved = FindTimeZone(timeZoneId);
        _defaultTimeZone = resolved ?? TimeZoneInfo.Utc;
        IsInitialized = resolved != null && resolved.Id != TimeZoneInfo.Utc.Id;
    }

    /// <summary>
    /// Set the timezone for the current request scope.
    /// Called by the timezone middleware.
    /// </summary>
    public static void SetRequestTimeZone(string timeZoneId)
    {
        _requestTimeZone.Value = FindTimeZone(timeZoneId);
    }

    /// <summary>
    /// Clear the per-request timezone (called at end of request).
    /// </summary>
    public static void ClearRequestTimeZone()
    {
        _requestTimeZone.Value = null;
    }

    /// <summary>
    /// The effective timezone: per-request if set, otherwise the configured default.
    /// </summary>
    public static TimeZoneInfo TimeZone => _requestTimeZone.Value ?? _defaultTimeZone;

    /// <summary>
    /// Returns the current <see cref="DateTimeOffset"/> in the effective timezone.
    /// </summary>
    public static DateTimeOffset Now
    {
        get
        {
            var utcNow = DateTimeOffset.UtcNow;
            return TimeZoneInfo.ConvertTime(utcNow, TimeZone);
        }
    }

    /// <summary>
    /// Returns the current <see cref="DateTime"/> in the effective timezone.
    /// </summary>
    public static DateTime DateTimeNow => Now.DateTime;

    /// <summary>
    /// Returns today's date in the effective timezone.
    /// </summary>
    public static DateTime Today => Now.DateTime.Date;

    /// <summary>
    /// True if <see cref="Initialize"/> resolved to a real timezone (not UTC fallback).
    /// </summary>
    public static bool IsInitialized { get; private set; }

    /// <summary>
    /// Try to resolve a timezone by Windows ID, IANA ID, or UTC offset string.
    /// Returns null if nothing matches.
    /// </summary>
    private static TimeZoneInfo? FindTimeZone(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // 1. Try standard system lookup (Windows ID or IANA depending on OS)
        try { return TimeZoneInfo.FindSystemTimeZoneById(input); }
        catch (TimeZoneNotFoundException) { }
        catch (InvalidTimeZoneException) { }

        // 2. Try converting Windows ID → IANA or IANA → Windows (.NET 6+)
        if (TimeZoneInfo.TryConvertIanaIdToWindowsId(input, out var windowsId))
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(windowsId); }
            catch { }
        }
        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(input, out var ianaId))
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(ianaId); }
            catch { }
        }

        // 3. Try parsing as UTC offset like "+02:00", "-05:00", "+04:00"
        input = input.Trim();
        if ((input.StartsWith('+') || input.StartsWith('-')) && TimeSpan.TryParse(input.Replace("+", ""), out var offset))
        {
            if (input.StartsWith('-'))
                offset = offset.Negate();

            return TimeZoneInfo.CreateCustomTimeZone(
                $"UTC{input}", offset, $"(UTC{input})", $"UTC{input}");
        }

        return null;
    }
}
