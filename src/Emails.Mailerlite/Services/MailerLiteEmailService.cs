using Emails.Mailerlite.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Emails.Mailerlite.Services;

public class MailerLiteEmailService : IEmailService
{
    private readonly MailerLiteSettings _settings;
    private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _webHostEnvironment;
    private readonly ILogger<MailerLiteEmailService> _logger;
    private readonly RestClient _client;

    public MailerLiteEmailService(
        MailerLiteSettings settings,
        Microsoft.AspNetCore.Hosting.IHostingEnvironment webHostEnvironment,
        ILogger<MailerLiteEmailService> logger)
    {
        if (string.IsNullOrWhiteSpace(settings?.ApiToken))
            throw new ArgumentException("MailerLite API token is required", nameof(settings));

        if (string.IsNullOrWhiteSpace(settings?.FromEmail))
            throw new ArgumentException("MailerLite FromEmail is required", nameof(settings));

        _settings = settings;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
        
        // MailerLite API base URL (API v2)
        var options = new RestClientOptions("https://connect.mailerlite.com/api");
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
    /// Internal method to send email via MailerLite API
    /// </summary>
    private async Task<bool> SendEmailInternalAsync(string to, List<string>? cc, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email via MailerLite - From: {FromEmail} ({FromName}), To: {To}, Subject: {Subject}",
                _settings.FromEmail, _settings.FromName ?? "Support", to, subject);

            // MailerLite API structure: Add subscriber to group then send campaign
            // Step 1: Add or update subscriber
            var subscriberRequest = new RestRequest("/subscribers", Method.Post);
            subscriberRequest.AddHeader("Authorization", $"Bearer {_settings.ApiToken}");
            subscriberRequest.AddHeader("Content-Type", "application/json");
            subscriberRequest.AddHeader("Accept", "application/json");

            var subscriberPayload = new
            {
                email = to,
                fields = new Dictionary<string, string>(),
                status = "active"
            };

            subscriberRequest.AddStringBody(JsonConvert.SerializeObject(subscriberPayload), ContentType.Json);
            
            var subscriberResponse = await _client.ExecuteAsync(subscriberRequest, cancellationToken);
            
            if (!subscriberResponse.IsSuccessful && subscriberResponse.StatusCode != System.Net.HttpStatusCode.Conflict)
            {
                _logger.LogError("Failed to add/update subscriber. Status: {StatusCode}, Error: {Error}",
                    subscriberResponse.StatusCode, subscriberResponse.Content);
                return false;
            }

            // Step 2: Create and send a campaign
            var request = new RestRequest("/campaigns", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_settings.ApiToken}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");

            var payload = new
            {
                name = $"{subject} - {DateTime.UtcNow:yyyyMMddHHmmss}",
                type = "regular",
                emails = new[]
                {
                    new
                    {
                        subject = subject,
                        from_name = _settings.FromName ?? "Support",
                        from = _settings.FromEmail,
                        content = htmlContent
                    }
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            request.AddStringBody(jsonPayload, ContentType.Json);

            var response = await _client.ExecuteAsync(request, cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogError("❌ MailerLite campaign creation failed. Status: {StatusCode}, To: {To}, Subject: {Subject}, Error: {Error}",
                    response.StatusCode, to, subject, response.Content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("UNAUTHORIZED (401) - MailerLite API token is invalid or expired. Please verify your API token in appsettings.json.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("FORBIDDEN (403) - Your MailerLite account might not have permission to send from {FromEmail}. " +
                        "Make sure the domain is verified in your MailerLite account.", _settings.FromEmail);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {
                    _logger.LogError("UNPROCESSABLE ENTITY (422) - The email request is invalid. Make sure FromEmail ({FromEmail}) is verified and formatted correctly.", 
                        _settings.FromEmail);
                }

                return false;
            }

            // Extract campaign ID from response
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(response.Content ?? "{}");
            string? campaignId = responseData?.data?.id?.ToString();

            if (string.IsNullOrEmpty(campaignId))
            {
                _logger.LogError("❌ Failed to extract campaign ID from response");
                return false;
            }

            _logger.LogInformation("✅ Campaign created successfully. ID: {CampaignId}. Now scheduling to send...", campaignId);

            // Step 3: Schedule/Send the campaign immediately
            // Use the 'actions' endpoint to send the campaign
            var sendRequest = new RestRequest($"/campaigns/{campaignId}/actions/send", Method.Post);
            sendRequest.AddHeader("Authorization", $"Bearer {_settings.ApiToken}");
            sendRequest.AddHeader("Content-Type", "application/json");
            sendRequest.AddHeader("Accept", "application/json");

            // Empty body for immediate send
            sendRequest.AddStringBody("{}", ContentType.Json);

            var sendResponse = await _client.ExecuteAsync(sendRequest, cancellationToken);

            if (sendResponse.IsSuccessful)
            {
                _logger.LogInformation("✅ MailerLite email sent successfully to {To}. Campaign ID: {CampaignId}",
                    to, campaignId);
                return true;
            }
            else
            {
                _logger.LogError("❌ Failed to send campaign. Status: {StatusCode}, Error: {Error}",
                    sendResponse.StatusCode, sendResponse.Content);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email via MailerLite to {To} with subject '{Subject}'", to, subject);
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
