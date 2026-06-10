using Contracting.Domain.Entities.helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Resources;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Text.Json;

namespace Contracting.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IStringLocalizer<SharedResources> _localizer;


    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IStringLocalizer<SharedResources> localizer)
    {
        _next = next;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await LogExceptionAsync(db, context, ex);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var problem = new { message = _localizer[SharedResourcesKeys.GlobalException] };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }
    }

    private async Task LogExceptionAsync(ApplicationDbContext db, HttpContext context, Exception ex)
    {
        try
        {
            var exceptionLog = new ExceptionLog
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                ExceptionType = ex.GetType().FullName,
                InnerExceptionMessage = ex.InnerException?.Message,
                InnerExceptionStackTrace = ex.InnerException?.StackTrace,
                HttpMethod = context.Request.Method,
                RequestPath = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("nameid")?.Value,
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            db.ExceptionLogs.Add(exceptionLog);
            await db.SaveChangesAsync();
        }
        catch (Exception logEx)
        {
            _logger.LogError(logEx, "Failed to log exception to database");
        }
    }
}
