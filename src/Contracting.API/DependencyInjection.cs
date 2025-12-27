using Contracting.API.Commen.Errors;
using Contracting.Application;
using Contracting.Shared.HelperDtos;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Globalization;

namespace Contracting.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers().AddFluentValidation();
            services.AddEndpointsApiExplorer();
            services.AddSingleton<ProblemDetailsFactory, ContractingProblemDetailsFactory>();

            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                List<CultureInfo> supportedCultures = new List<CultureInfo>
                  {
                      new CultureInfo("ar"),
                      new CultureInfo("en")
                  };

                options.DefaultRequestCulture = new RequestCulture("ar");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders.Insert(
                    0,
                    new AcceptLanguageHeaderRequestCultureProvider()
                );
            });
            services.Configure<FirebaseSettings>(configuration.GetSection("Firebase"));
            return services;
        }
    }
}
