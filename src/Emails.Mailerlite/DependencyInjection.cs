using Emails.Mailerlite.Models;
using Emails.Mailerlite.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emails.Mailerlite;

public static class DependencyInjection
{
    /// <summary>
    /// Registers MailerLite email service with dependency injection
    /// </summary>
    public static IServiceCollection AddMailerLiteEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var mailerLiteSettings = new MailerLiteSettings();
        configuration.GetSection("MailerLiteSettings").Bind(mailerLiteSettings);

        if (string.IsNullOrWhiteSpace(mailerLiteSettings.ApiToken))
            throw new InvalidOperationException("MailerLite API token is not configured. Please add 'MailerLiteSettings:ApiToken' to configuration.");

        if (string.IsNullOrWhiteSpace(mailerLiteSettings.FromEmail))
            throw new InvalidOperationException("MailerLite FromEmail is not configured. Please add 'MailerLiteSettings:FromEmail' to configuration.");

        if (string.IsNullOrWhiteSpace(mailerLiteSettings.FromName))
            throw new InvalidOperationException("MailerLite FromName is not configured. Please add 'MailerLiteSettings:FromName' to configuration.");

        services.AddSingleton(mailerLiteSettings);
        services.AddScoped<IEmailService, MailerLiteEmailService>();

        return services;
    }
}
