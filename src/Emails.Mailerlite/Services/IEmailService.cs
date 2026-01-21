namespace Emails.Mailerlite.Services;

public interface IEmailService
{
    /// <summary>
    /// Sends an email with HTML content loaded from a file in wwwroot
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlFileName">HTML file name (from wwwroot/emails folder)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendEmailAsync(string to, string subject, string htmlFileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email with HTML content and email cc recipients
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="cc">CC recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlFileName">HTML file name (from wwwroot/emails folder)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendEmailAsync(string to, List<string> cc, string subject, string htmlFileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email with HTML content and subject line
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlFileName">HTML file name (from wwwroot/emails folder)</param>
    /// <param name="replacements">Key-value pairs for replacing placeholders in HTML (e.g., {{UserName}} -> actual value)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendEmailAsync(string to, string subject, string htmlFileName, Dictionary<string, string> replacements, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email with HTML content, cc recipients, and template replacements
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="cc">CC recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlFileName">HTML file name (from wwwroot/emails folder)</param>
    /// <param name="replacements">Key-value pairs for replacing placeholders in HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendEmailAsync(string to, List<string> cc, string subject, string htmlFileName, Dictionary<string, string> replacements, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends bulk emails to multiple recipients with the same template
    /// </summary>
    /// <param name="recipients">List of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlFileName">HTML file name (from wwwroot/emails folder)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string htmlFileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends bulk emails with individual replacements per recipient
    /// </summary>
    /// <param name="recipients">Dictionary mapping email addresses to their individual replacements</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlFileName">HTML file name (from wwwroot/emails folder)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendBulkEmailAsync(Dictionary<string, Dictionary<string, string>> recipients, string subject, string htmlFileName, CancellationToken cancellationToken = default);
}
