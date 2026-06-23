using Contracting.API.Commen.Errors;
using Contracting.Shared.Dtos.HelperDtos;
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
            services.AddControllers()
                .AddFluentValidation()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });
            services.AddEndpointsApiExplorer();
            services.AddSingleton<ProblemDetailsFactory, ContractingProblemDetailsFactory>();

            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                // UI cultures drive localized resource strings (Arabic + English).
                List<CultureInfo> supportedUICultures = new List<CultureInfo>
                  {
                      new CultureInfo("ar"),
                      new CultureInfo("en")
                  };

                // Formatting culture drives number/date PARSING during model binding.
                // Force "en" so decimals always bind with "." as the separator
                // (Arabic culture uses "٫", which breaks values like 33.22 on form posts).
                List<CultureInfo> supportedFormattingCultures = new List<CultureInfo>
                  {
                      new CultureInfo("en")
                  };

                options.DefaultRequestCulture = new RequestCulture(culture: "en", uiCulture: "ar");
                options.SupportedCultures = supportedFormattingCultures;
                options.SupportedUICultures = supportedUICultures;
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
