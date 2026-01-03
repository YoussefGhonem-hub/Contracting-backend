using Logging.Serilog.Dtos;
using Logging.Serilog.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Diagnostics;

namespace Logging.Serilog;

/// <summary>
/// Centralized Serilog bootstrapping and HTTP logging wiring used by services.
/// </summary>
/// <remarks>
/// - Configures a bootstrap logger early for startup errors.
/// - Builds the final logger from appsettings (enrichment + sinks).
/// - Binds request logging options and wires correlation propagation.
/// - Adds request/response logging middleware and optional Serilog request summaries.
/// </remarks>
public static class SerilogBootstrap
{
    /// <summary>
    /// Registers and configures Serilog for the current service, including:
    /// - a bootstrap console logger for early startup failures,
    /// - the final logger pipeline from configuration and DI,
    /// - request logging options binding,
    /// - correlation id forwarding for outbound <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="serviceName">
    /// Optional logical service name to enrich log events with. Defaults to the application name.
    /// </param>
    public static void AddSharedSerilog(this WebApplicationBuilder builder, string? serviceName = null)
    {
        // Bootstrap logger for startup errors (before Host is built)
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Service", serviceName ?? builder.Environment.ApplicationName)
            .WriteTo.Async(x => x.Console(new CompactJsonFormatter()))
            .CreateLogger();

        // Build the real logger from THIS service's appsettings and DI
        builder.Host.UseSerilog((ctx, services, lc) =>
        {
            lc.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithMachineName()
              .Enrich.WithProcessId()
              .Enrich.WithThreadId()
              .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
              .Enrich.WithProperty("Service", serviceName ?? builder.Environment.ApplicationName);
        });

        // Bind request logging options (whitelist, body logging, etc.)
        var opt = new RequestLoggingOptions();
        builder.Configuration.GetSection("RequestLogging").Bind(opt);
        builder.Services.AddSingleton(opt);

        // Correlation id propagation for downstream HTTP calls
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdHandler>();
        builder.Services.AddHttpClient("default").AddHttpMessageHandler<CorrelationIdHandler>();

        // Also expose options via IOptions<T> for consumers
        builder.Services.Configure<RequestLoggingOptions>(
            builder.Configuration.GetSection("RequestLogging"));
    }

    /// <summary>
    /// Adds HTTP request/response logging to the pipeline:
    /// - Inserts <see cref="HttpLoggingMiddleware"/> to enrich logs, capture bodies (when enabled/whitelisted),
    ///   and log exceptions in a structured way.
    /// - Optionally enables Serilog's built-in request logging summaries based on options.
    /// </summary>
    /// <param name="app">The configured web application.</param>
    /// <returns>The same application for chaining.</returns>
    public static WebApplication UseSharedHttpLogging(this WebApplication app)
    {
        // Structured request/response logging with redaction and correlation
        app.UseMiddleware<HttpLoggingMiddleware>();

        var opt = app.Services.GetRequiredService<IOptions<RequestLoggingOptions>>().Value;

        // Optional Serilog request summaries (fast one-line logs)
        if (opt.LogAllRequests || (opt.PathGlobsForRequests?.Length ?? 0) > 0)
        {
            app.UseSerilogRequestLogging(o =>
            {
                o.EnrichDiagnosticContext = (ctx, http) =>
                {
                    ctx.Set("TraceId", Activity.Current?.Id ?? http.TraceIdentifier);
                    ctx.Set("UserId", http.User?.Identity?.IsAuthenticated == true ? http.User.Identity!.Name : "anonymous");
                    ctx.Set("RemoteIp", http.Connection.RemoteIpAddress?.ToString());
                    ctx.Set("Route", http.Request.Path);
                    ctx.Set("QueryString", http.Request.QueryString.Value);
                };
            });
        }
        return app;
    }
}