using Emails.Mailersend.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Emails.Mailersend.Services;

public class MailerSendEmailService : IEmailService
{
    private readonly MailerSendSettings _settings;
    private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _webHostEnvironment;
    private readonly ILogger<MailerSendEmailService> _logger;
    private readonly RestClient _client;

    public MailerSendEmailService(
        MailerSendSettings settings,
        Microsoft.AspNetCore.Hosting.IHostingEnvironment webHostEnvironment,
        ILogger<MailerSendEmailService> logger)
    {
        if (string.IsNullOrWhiteSpace(settings?.ApiToken))
            throw new ArgumentException("MailerSend API token is required", nameof(settings));

        if (string.IsNullOrWhiteSpace(settings?.FromEmail))
            throw new ArgumentException("MailerSend FromEmail is required", nameof(settings));

        _settings = settings;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
        
        var options = new RestClientOptions("https://api.mailersend.com/v1");
        _client = new RestClient(options);
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
    /// Internal method to send email via MailerSend API
    /// </summary>
    private async Task<bool> SendEmailInternalAsync(string to, List<string>? cc, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email via MailerSend - From: {FromEmail} ({FromName}), To: {To}, Subject: {Subject}",
                _settings.FromEmail, _settings.FromName ?? "Support", to, subject);

            var request = new RestRequest("/email", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_settings.ApiToken}");
            request.AddHeader("Content-Type", "application/json");

            // Build the email payload according to MailerSend API documentation
            object payload;
            
            if (cc != null && cc.Any())
            {
                payload = new
                {
                    from = new
                    {
                        email = _settings.FromEmail,
                        name = _settings.FromName ?? "Support"
                    },
                    to = new[]
                    {
                        new { email = to }
                    },
                    cc = cc.Select(email => new { email }).ToArray(),
                    subject = subject,
                    html = htmlContent
                };
            }
            else
            {
                payload = new
                {
                    from = new
                    {
                        email = _settings.FromEmail,
                        name = _settings.FromName ?? "Support"
                    },
                    to = new[]
                    {
                        new { email = to }
                    },
                    subject = subject,
                    html = htmlContent
                };
            }

            var jsonPayload = JsonConvert.SerializeObject(payload);
            request.AddStringBody(jsonPayload, ContentType.Json);

            var response = await _client.ExecuteAsync(request, cancellationToken);

            if (response.IsSuccessful)
            {
                _logger.LogInformation("✅ MailerSend email sent successfully to {To}. Status: {StatusCode}",
                    to, response.StatusCode);
                return true;
            }
            else
            {
                _logger.LogError("❌ MailerSend email failed. Status: {StatusCode}, To: {To}, Subject: {Subject}, Error: {Error}",
                    response.StatusCode, to, subject, response.Content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("UNAUTHORIZED (401) - MailerSend API token is invalid or expired. Please verify your API token in appsettings.json.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("FORBIDDEN (403) - Your MailerSend account might not have permission to send from {FromEmail}. " +
                        "Make sure the domain is verified in your MailerSend account.", _settings.FromEmail);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {
                    _logger.LogError("UNPROCESSABLE ENTITY (422) - The email request is invalid. Make sure FromEmail ({FromEmail}) is verified and formatted correctly.", 
                        _settings.FromEmail);
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email via MailerSend to {To} with subject '{Subject}'", to, subject);
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
