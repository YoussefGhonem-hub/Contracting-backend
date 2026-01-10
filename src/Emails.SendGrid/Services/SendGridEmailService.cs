using Emails.SendGrid.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Emails.SendGrid.Services;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridClient _sendGridClient;
    private readonly SendGridSettings _settings;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SendGridEmailService(SendGridSettings settings, IWebHostEnvironment webHostEnvironment)
    {
        if (string.IsNullOrWhiteSpace(settings?.ApiKey))
            throw new ArgumentException("SendGrid API key is required", nameof(settings));

        _settings = settings;
        _sendGridClient = new SendGridClient(settings.ApiKey);
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// Sends an email with HTML content loaded from a file in wwwroot
    /// </summary>
    public async Task<bool> SendEmailAsync(string to, string subject, string htmlFileName, CancellationToken cancellationToken = default)
    {
        var htmlContent = await LoadHtmlFileAsync(htmlFileName);
        return await SendEmailInternalAsync(to, null, subject, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends an email with HTML content and cc recipients
    /// </summary>
    public async Task<bool> SendEmailAsync(string to, List<string> cc, string subject, string htmlFileName, CancellationToken cancellationToken = default)
    {
        var htmlContent = await LoadHtmlFileAsync(htmlFileName);
        return await SendEmailInternalAsync(to, cc, subject, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends an email with HTML content and template replacements
    /// </summary>
    public async Task<bool> SendEmailAsync(string to, string subject, string htmlFileName, Dictionary<string, string> replacements, CancellationToken cancellationToken = default)
    {
        var htmlContent = await LoadHtmlFileAsync(htmlFileName);
        htmlContent = ApplyReplacements(htmlContent, replacements);
        return await SendEmailInternalAsync(to, null, subject, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends an email with HTML content, cc recipients, and template replacements
    /// </summary>
    public async Task<bool> SendEmailAsync(string to, List<string> cc, string subject, string htmlFileName, Dictionary<string, string> replacements, CancellationToken cancellationToken = default)
    {
        var htmlContent = await LoadHtmlFileAsync(htmlFileName);
        htmlContent = ApplyReplacements(htmlContent, replacements);
        return await SendEmailInternalAsync(to, cc, subject, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends bulk emails to multiple recipients with the same template
    /// </summary>
    public async Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string htmlFileName, CancellationToken cancellationToken = default)
    {
        var htmlContent = await LoadHtmlFileAsync(htmlFileName);
        var tasks = recipients.Select(to => SendEmailInternalAsync(to, null, subject, htmlContent, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    /// <summary>
    /// Sends bulk emails with individual replacements per recipient
    /// </summary>
    public async Task<bool> SendBulkEmailAsync(Dictionary<string, Dictionary<string, string>> recipients, string subject, string htmlFileName, CancellationToken cancellationToken = default)
    {
        var baseHtmlContent = await LoadHtmlFileAsync(htmlFileName);
        var tasks = recipients.Select(async kvp =>
        {
            var to = kvp.Key;
            var replacements = kvp.Value;
            var htmlContent = ApplyReplacements(baseHtmlContent, replacements);
            return await SendEmailInternalAsync(to, null, subject, htmlContent, cancellationToken);
        });

        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    /// <summary>
    /// Internal method to send email via SendGrid
    /// </summary>
    private async Task<bool> SendEmailInternalAsync(string to, List<string>? cc, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(_settings.FromEmail, _settings.FromName ?? "Support");
            var toEmail = new EmailAddress(to);

            var msg = new SendGridMessage()
            {
                From = from,
                Subject = subject,
                HtmlContent = htmlContent
            };

            msg.AddTo(toEmail);

            if (cc != null && cc.Any())
            {
                foreach (var ccEmail in cc)
                {
                    msg.AddCc(new EmailAddress(ccEmail));
                }
            }

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);
            return response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            // Log error (integrate with your logging if needed)
            return false;
        }
    }

    /// <summary>
    /// Loads HTML content from a file in wwwroot/emails
    /// </summary>
    private async Task<string> LoadHtmlFileAsync(string htmlFileName)
    {
        try
        {
            var wwwrootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(wwwrootPath))
                throw new InvalidOperationException("WebRootPath is not configured.");

            var filePath = Path.Combine(wwwrootPath, "emails", htmlFileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Email template not found: {filePath}");

            var htmlContent = await File.ReadAllTextAsync(filePath);
            return htmlContent;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading email template '{htmlFileName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Applies key-value replacements to HTML content (e.g., {{UserName}} -> actual value)
    /// </summary>
    private string ApplyReplacements(string htmlContent, Dictionary<string, string>? replacements)
    {
        if (replacements == null || !replacements.Any())
            return htmlContent;

        var result = htmlContent;
        foreach (var kvp in replacements)
        {
            // Replace both {{key}} and {{Key}} patterns
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
