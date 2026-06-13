using Emails.Mailersend.Models;
using Emails.Mailersend.Services;
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
    /// Test endpoint to verify MailerSend configuration and email sending
    /// </summary>
    /// <param name="testEmail">Email address to send test email to</param>
    /// <returns>Success or error message</returns>
    [HttpPost("send-test")]
    [AllowAnonymous]
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
                "Test Email - MailerSend Configuration Test",
                "reset-password-code.html",
                replacements);

            if (result)
            {
                _logger.LogInformation("Test email sent successfully to {Email}", testEmail);
                return Ok(new
                {
                    success = true,
                    message = $"✅ MailerSend successfully sent email to {testEmail}",
                    nextSteps = new[]
                    {
                        $"Check {testEmail} inbox (including spam/junk folder)",
                        "Check MailerSend activity: https://app.mailersend.com/",
                        "Verify your sending domain at: https://app.mailersend.com/domains"
                    }
                });
            }
            else
            {
                _logger.LogWarning("Failed to send test email to {Email}", testEmail);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send email. Check the application logs for details.",
                    troubleshooting = new[]
                    {
                        "1. Verify your sending domain is verified in MailerSend (https://app.mailersend.com/domains)",
                        "2. Check your MailerSend API token in MailerSendSettings:ApiToken",
                        "3. Ensure FromEmail matches a verified domain in MailerSend",
                        "4. Check application logs for the detailed API error response"
                    }
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
    /// Check if MailerSend email service is configured correctly
    /// </summary>
    [HttpGet("configuration-check")]
    [AllowAnonymous]
    public IActionResult CheckConfiguration([FromServices] MailerSendSettings settings)
    {
        try
        {
            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(settings.ApiToken))
                issues.Add("ApiToken is missing");
            else if (settings.ApiToken.Length < 20)
                issues.Add("ApiToken appears to be invalid (too short)");

            if (string.IsNullOrWhiteSpace(settings.FromEmail))
                issues.Add("FromEmail is missing");
            else if (!settings.FromEmail.Contains("@"))
                issues.Add("FromEmail is not a valid email format");
            else if (settings.FromEmail.Trim() != settings.FromEmail)
                issues.Add("⚠️ FromEmail has leading/trailing spaces!");

            if (string.IsNullOrWhiteSpace(settings.FromName))
                issues.Add("FromName is missing");

            if (issues.Any())
            {
                return Ok(new
                {
                    success = false,
                    message = "MailerSend configuration has issues",
                    issues,
                    configuration = new
                    {
                        ApiTokenPrefix = settings.ApiToken?.Substring(0, Math.Min(15, settings.ApiToken?.Length ?? 0)) + "***",
                        FromEmail = settings.FromEmail,
                        FromName = settings.FromName
                    }
                });
            }

            return Ok(new
            {
                success = true,
                message = "✅ MailerSend configuration loaded successfully",
                configuration = new
                {
                    ApiTokenPrefix = settings.ApiToken?.Substring(0, Math.Min(15, settings.ApiToken?.Length ?? 0)) + "***",
                    FromEmail = settings.FromEmail,
                    FromName = settings.FromName
                },
                nextSteps = new[]
                {
                    "1. Make sure your sending domain is verified in MailerSend dashboard",
                    "2. Use POST /api/EmailTest/send-test?testEmail=your@email.com to test",
                    "3. Check application logs for detailed error information",
                    "4. Verify domain at: https://app.mailersend.com/domains"
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "MailerSend configuration error",
                error = ex.Message
            });
        }
    }
}
