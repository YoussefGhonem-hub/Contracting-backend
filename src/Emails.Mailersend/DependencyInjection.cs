using Emails.Mailersend.Models;
using Emails.Mailersend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emails.Mailersend;

public static class DependencyInjection
{
    /// <summary>
    /// Registers MailerSend email service with dependency injection
    /// </summary>
    public static IServiceCollection AddMailerSendEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var mailerSendSettings = new MailerSendSettings();
        configuration.GetSection("MailerSendSettings").Bind(mailerSendSettings);

        if (string.IsNullOrWhiteSpace(mailerSendSettings.ApiToken))
            throw new InvalidOperationException("MailerSend API token is not configured. Please add 'MailerSendSettings:ApiToken' to configuration.");

        if (string.IsNullOrWhiteSpace(mailerSendSettings.FromEmail))
            throw new InvalidOperationException("MailerSend FromEmail is not configured. Please add 'MailerSendSettings:FromEmail' to configuration.");

        if (string.IsNullOrWhiteSpace(mailerSendSettings.FromName))
            throw new InvalidOperationException("MailerSend FromName is not configured. Please add 'MailerSendSettings:FromName' to configuration.");

        services.AddSingleton(mailerSendSettings);
        services.AddScoped<IEmailService, MailerSendEmailService>();

        return services;
    }
}
