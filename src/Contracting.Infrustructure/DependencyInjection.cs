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
using Contracting.Shared.Security;
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
        // Connection string may be "ENC:..." (encrypted for production) or plaintext (local dev) —
        // Decrypt() returns non-encrypted values unchanged, so both work with the same code path.
        var encryptionKey = configuration["Encryption:Key"];
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString) && AppSettingsProtector.IsEncrypted(connectionString))
        {
            if (string.IsNullOrWhiteSpace(encryptionKey))
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is encrypted but 'Encryption:Key' is missing from configuration.");
            connectionString = AppSettingsProtector.Decrypt(connectionString, encryptionKey);
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
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
        services.AddScoped<IRequestTypeDefaultDepartmentService, RequestTypeDefaultDepartmentService>();
        services.AddScoped<IStatueService, StatueService>();
        services.AddScoped<IEngineerRequestService, EngineerRequestService>();
        services.AddScoped<IEngineerRequestAnalysisService, EngineerRequestAnalysisService>();
            services.AddScoped<IEngineerSiteSurveyQuestionService, EngineerSiteSurveyQuestionService>();
        services.AddScoped<IEngineerSiteReportService, EngineerSiteReportService>();
        services.AddScoped<ITechnicalVariationOrderService, TechnicalVariationOrderService>();
        services.AddScoped<IConstructionItemService, ConstructionItemService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IClientProjectService, ClientProjectService>();
        services.AddScoped<IClientSiteReportService, ClientSiteReportService>();
        services.AddScoped<IClientInvoiceService, ClientInvoiceService>();
        services.AddScoped<IClientTenderService, ClientTenderService>();
        services.AddScoped<IClientVariationOrderService, ClientVariationOrderService>();
        services.AddScoped<IClientScheduleService, ClientScheduleService>();
        services.AddScoped<IClientDrawingService, ClientDrawingService>();
        services.AddScoped<IThreeDFolderService, ThreeDFolderService>();
        services.AddScoped<IPerformanceAnalyticsService, PerformanceAnalyticsService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IFirebaseService, FirebaseService>();
        services.AddScoped<ITransferRequestService, TransferRequestService>();
        services.AddScoped<IFinancialClearanceService, FinancialClearanceService>();
        services.AddScoped<ILaborAttendanceService, LaborAttendanceService>();
        services.AddScoped<IClientContentService, ClientContentService>();

        services.AddHttpContextAccessor();

        // Firebase options
        services.Configure<FirebaseOptions>(configuration.GetSection("Firebase"));

        // File storage
        services.AddScoped<IFileStorage, S3FileStorage>();

        return services;
    }
}
