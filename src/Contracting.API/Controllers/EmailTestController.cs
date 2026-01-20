using Emails.SendGrid.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

/// <summary>
/// Controller for testing email functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailTestController> _logger;

    public EmailTestController(IEmailService emailService, ILogger<EmailTestController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint to verify SendGrid configuration and email sending
    /// </summary>
    /// <param name="testEmail">Email address to send test email to</param>
    /// <returns>Success or error message</returns>
    [HttpPost("send-test")]
    [AllowAnonymous] // Remove this in production
    public async Task<IActionResult> SendTestEmail([FromQuery] string testEmail)
    {
        if (string.IsNullOrWhiteSpace(testEmail))
        {
            return BadRequest(new { success = false, message = "Please provide a valid email address" });
        }

        try
        {
            _logger.LogInformation("Attempting to send test email to {Email}", testEmail);

            var replacements = new Dictionary<string, string>
            {
                { "UserName", "Test User" },
                { "VerificationCode", "123456" },
                { "ExpiryMinutes", "15" }
            };

            var result = await _emailService.SendEmailAsync(
                testEmail,
                "Test Email - SendGrid Configuration Test",
                "reset-password-code.html",
                replacements);

            if (result)
            {
                _logger.LogInformation("Test email sent successfully to {Email}", testEmail);
                return Ok(new
                {
                    success = true,
                    message = $"SendGrid ACCEPTED the email to {testEmail}",
                    important = new[]
                    {
                        "⚠️ ACCEPTED does NOT mean DELIVERED!",
                        "SendGrid accepted the request but may not deliver if sender is not verified.",
                        $"Check if email arrived at {testEmail} (including spam/junk folder)",
                        "Check SendGrid Activity: https://app.sendgrid.com/email_activity",
                        "If not delivered: Verify 'noreply@contracting.app' in SendGrid > Settings > Sender Authentication"
                    },
                    nextSteps = new[]
                    {
                        "1. Check the application logs for SendGrid Message ID",
                        "2. Go to https://app.sendgrid.com/email_activity and search for the message",
                        "3. Look for delivery status, bounces, or blocks",
                        "4. Verify sender email if status shows 'not authenticated'"
                    }
                });
            }
            else
            {
                _logger.LogWarning("Failed to send test email to {Email}", testEmail);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send email. Check the application logs for details."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending test email to {Email}", testEmail);
            return StatusCode(500, new
            {
                success = false,
                message = $"Error: {ex.Message}",
                details = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Check if email template file exists
    /// </summary>
    [HttpGet("check-template")]
    [AllowAnonymous]
    public IActionResult CheckTemplate([FromServices] IWebHostEnvironment env)
    {
        var templatePath = Path.Combine(env.WebRootPath ?? "", "emails", "reset-password-code.html");
        var exists = System.IO.File.Exists(templatePath);

        return Ok(new
        {
            templatePath,
            exists,
            message = exists 
                ? "✅ Email template found" 
                : "❌ Email template NOT found - Create this file or emails will fail!"
        });
    }

    /// <summary>
    /// Simple test to check if email service is configured correctly
    /// </summary>
    [HttpGet("configuration-check")]
    [AllowAnonymous] // Remove this in production
    public IActionResult CheckConfiguration([FromServices] Emails.SendGrid.Models.SendGridSettings settings)
    {
        try
        {
            var issues = new List<string>();
            
            if (string.IsNullOrWhiteSpace(settings.ApiKey))
                issues.Add("ApiKey is missing");
            else if (settings.ApiKey.Length < 20)
                issues.Add("ApiKey appears to be invalid (too short)");
                
            if (string.IsNullOrWhiteSpace(settings.FromEmail))
                issues.Add("FromEmail is missing");
            else if (!settings.FromEmail.Contains("@"))
                issues.Add("FromEmail is not a valid email format");
            else if (settings.FromEmail.Trim() != settings.FromEmail)
                issues.Add("⚠️ FromEmail has leading/trailing spaces!");
                
            if (string.IsNullOrWhiteSpace(settings.FromName))
                issues.Add("FromName is missing");

            // Check for exact match with verified sender
            var verifiedSender = "youssef.fcih@gmail.com";
            var exactMatch = settings.FromEmail?.Equals(verifiedSender, StringComparison.Ordinal) ?? false;

            if (issues.Any())
            {
                return Ok(new
                {
                    success = false,
                    message = "SendGrid configuration has issues",
                    issues = issues,
                    configuration = new
                    {
                        ApiKeyPrefix = settings.ApiKey?.Substring(0, Math.Min(15, settings.ApiKey?.Length ?? 0)) + "***",
                        FromEmail = settings.FromEmail,
                        FromEmailLength = settings.FromEmail?.Length ?? 0,
                        FromName = settings.FromName,
                        ExactMatchWithVerified = exactMatch,
                        VerifiedSenderInSendGrid = verifiedSender
                    }
                });
            }

            return Ok(new
            {
                success = true,
                message = exactMatch 
                    ? "✅ Configuration matches verified sender!" 
                    : "⚠️ Configuration loaded but FromEmail may not match verified sender",
                configuration = new
                {
                    ApiKeyPrefix = settings.ApiKey?.Substring(0, Math.Min(15, settings.ApiKey?.Length ?? 0)) + "***",
                    FromEmail = settings.FromEmail,
                    FromEmailLength = settings.FromEmail?.Length ?? 0,
                    FromName = settings.FromName,
                    ExactMatchWithVerified = exactMatch,
                    VerifiedSenderInSendGrid = verifiedSender,
                    CharComparison = exactMatch ? "✅ Exact match" : $"❌ '{settings.FromEmail}' != '{verifiedSender}'"
                },
                nextSteps = new[]
                {
                    exactMatch 
                        ? "✅ FromEmail matches verified sender - should work!" 
                        : $"❌ Update FromEmail in appsettings.json to exactly: {verifiedSender}",
                    "1. Restart the application after config changes",
                    "2. Use POST /api/EmailTest/send-test?testEmail=your@email.com to test",
                    "3. Check application logs for detailed 403 error information"
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "SendGrid configuration error",
                error = ex.Message
            });
        }
    }
}
