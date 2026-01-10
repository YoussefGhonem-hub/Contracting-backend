using Emails.SendGrid.Models;
using Emails.SendGrid.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emails.SendGrid;

public static class DependencyInjection
{
    /// <summary>
    /// Registers SendGrid email service with dependency injection
    /// </summary>
    public static IServiceCollection AddSendGridEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var sendGridSettings = new SendGridSettings();
        configuration.GetSection("SendGridSettings").Bind(sendGridSettings);

        if (string.IsNullOrWhiteSpace(sendGridSettings.ApiKey))
            throw new InvalidOperationException("SendGrid API key is not configured. Please add 'SendGridSettings:ApiKey' to configuration.");

        services.AddSingleton(sendGridSettings);
        services.AddScoped<IEmailService, SendGridEmailService>();

        return services;
    }

    /// <summary>
    /// Registers SendGrid email service with explicit API key and settings
    /// </summary>
    public static IServiceCollection AddSendGridEmail(this IServiceCollection services, string apiKey, string? fromEmail = null, string? fromName = null)
    {
        var sendGridSettings = new SendGridSettings
        {
            ApiKey = apiKey,
            FromEmail = fromEmail ?? "noreply@contracting.app",
            FromName = fromName ?? "Contracting System"
        };

        services.AddSingleton(sendGridSettings);
        services.AddScoped<IEmailService, SendGridEmailService>();

        return services;
    }
}
