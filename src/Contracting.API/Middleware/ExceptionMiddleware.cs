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

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var problem = new { message = _localizer[SharedResourcesKeys.GlobalException] };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
