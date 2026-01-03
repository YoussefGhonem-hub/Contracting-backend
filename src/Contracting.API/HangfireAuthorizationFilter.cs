using Hangfire.Dashboard;

namespace Contracting.API
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

                 // Allow any authenticated user to access the dashboard
                 return httpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}
