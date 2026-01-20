using Emails.SendGrid.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Emails.SendGrid.Services;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridClient _sendGridClient;
    private readonly SendGridSettings _settings;
    private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _webHostEnvironment;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(SendGridSettings settings, Microsoft.AspNetCore.Hosting.IHostingEnvironment webHostEnvironment, ILogger<SendGridEmailService> logger)
    {
        if (string.IsNullOrWhiteSpace(settings?.ApiKey))
            throw new ArgumentException("SendGrid API key is required", nameof(settings));

        if (string.IsNullOrWhiteSpace(settings?.FromEmail))
            throw new ArgumentException("SendGrid FromEmail is required and must be verified in your SendGrid account", nameof(settings));

        _settings = settings;
        _sendGridClient = new SendGridClient(settings.ApiKey);
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
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
            _logger.LogWarning("🔍 DIAGNOSTIC - About to send email:");
            _logger.LogWarning("   From: '{FromEmail}' (Name: '{FromName}')", _settings.FromEmail, _settings.FromName);
            _logger.LogWarning("   To: '{To}'", to);
            _logger.LogWarning("   Subject: '{Subject}'", subject);
            _logger.LogWarning("   API Key Prefix: {ApiKeyPrefix}***", _settings.ApiKey?.Substring(0, Math.Min(15, _settings.ApiKey?.Length ?? 0)));

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
            var isSuccess = response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.OK;
            var responseBody = await response.Body.ReadAsStringAsync();

            // Extract SendGrid Message ID from headers
            string? messageId = null;
            if (response.Headers.TryGetValues("X-Message-Id", out var messageIds))
            {
                messageId = messageIds.FirstOrDefault();
            }

            if (!isSuccess)
            {
                // Special handling for common errors
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("UNAUTHORIZED (401) - SendGrid API Key is invalid or expired. " +
                        "Please verify your API key in appsettings.json. " +
                        "Current FromEmail: {FromEmail}, Response: {Response}",
                        _settings.FromEmail, responseBody);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("❌ FORBIDDEN (403) ERROR - SendGrid is BLOCKING your email!");
                    _logger.LogError("   FromEmail being used: '{FromEmail}'", _settings.FromEmail);
                    _logger.LogError("   Verified senders in SendGrid: Check https://app.sendgrid.com/settings/sender_auth/senders");
                    _logger.LogError("   Response from SendGrid: {Response}", responseBody);
                    _logger.LogError("");
                    _logger.LogError("🔧 SOLUTIONS:");
                    _logger.LogError("   1. Make sure '{FromEmail}' is EXACTLY verified in SendGrid (case-sensitive, no extra spaces)", _settings.FromEmail);
                    _logger.LogError("   2. Check if you need Domain Authentication instead of Single Sender");
                    _logger.LogError("   3. Verify your SendGrid account is not suspended or restricted");
                    _logger.LogError("   4. Try using the EXACT email from your verified sender: youssef.fcih@gmail.com");
                }
                else
                {
                    _logger.LogError("SendGrid email failed. Status: {StatusCode}, To: {To}, Subject: {Subject}, FromEmail: {FromEmail}, Response: {Response}",
                        response.StatusCode, to, subject, _settings.FromEmail, responseBody);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ SendGrid ACCEPTED request (Status: {StatusCode}) for email to {To}. " +
                    "MessageId: {MessageId}. " +
                    "⚠️ IMPORTANT: This does NOT mean the email was delivered! " +
                    "Check SendGrid Activity (https://app.sendgrid.com/email_activity) to verify delivery. " +
                    "Common reasons for non-delivery: FromEmail '{FromEmail}' not verified, recipient blocked you, spam filters.",
                    response.StatusCode, to, messageId ?? "N/A", _settings.FromEmail);
            }

            return isSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email to {To} with subject '{Subject}'. From: {FromEmail}, ApiKey: {ApiKeyPrefix}***", 
                to, subject, _settings.FromEmail, _settings.ApiKey?.Substring(0, Math.Min(10, _settings.ApiKey?.Length ?? 0)));
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
