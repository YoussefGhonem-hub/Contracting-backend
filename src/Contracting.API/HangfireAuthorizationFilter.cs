using Hangfire.Dashboard;

namespace Contracting.API
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _env;

    public HangfireAuthorizationFilter(IWebHostEnvironment env)
    {
        _env = env;
    }

    public bool Authorize(DashboardContext context)
    {
        if (_env.IsDevelopment())
        {
            return true; // allow local dev box
        }

        var httpContext = context.GetHttpContext();
        return httpContext?.User?.Identity?.IsAuthenticated == true;
    }
}
}
