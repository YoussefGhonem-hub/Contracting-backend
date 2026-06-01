using Contracting.Domain.Entities;
using Contracting.Infrustructure.Features;
using Contracting.Infrustructure.Features.business;
using Contracting.Infrustructure.Features.client;
using Contracting.Infrustructure.Features.Firebase;
using Contracting.Infrustructure.Features.Helper;
using Contracting.Infrustructure.Files;
using Contracting.Infrustructure.Identity;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contracting.Infrustructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sql.CommandTimeout(300);
                }));

        services.AddScoped<ApplicationDbContext>();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IEngineerService, EngineerService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IPriorityService, PriorityService>();
        services.AddScoped<IStatueService, StatueService>();
        services.AddScoped<IEngineerRequestService, EngineerRequestService>();
        services.AddScoped<IEngineerRequestAnalysisService, EngineerRequestAnalysisService>();
            services.AddScoped<IEngineerSiteSurveyQuestionService, EngineerSiteSurveyQuestionService>();
        services.AddScoped<IEngineerSiteReportService, EngineerSiteReportService>();
        services.AddScoped<IConstructionItemService, ConstructionItemService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IClientProjectService, ClientProjectService>();
        services.AddScoped<IClientSiteReportService, ClientSiteReportService>();
        services.AddScoped<IClientInvoiceService, ClientInvoiceService>();
        services.AddScoped<IClientTenderService, ClientTenderService>();
        services.AddScoped<IClientVariationOrderService, ClientVariationOrderService>();
        services.AddScoped<IClientScheduleService, ClientScheduleService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IFirebaseService, FirebaseService>();

        services.AddHttpContextAccessor();

        // Firebase options
        services.Configure<FirebaseOptions>(configuration.GetSection("Firebase"));

        // File storage
        services.AddScoped<IFileStorage, LocalFileStorage>();

        return services;
    }
}
