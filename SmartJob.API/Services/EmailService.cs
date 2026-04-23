using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SmartJob.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var emailConfig = _config.GetSection("Email");
        var host = emailConfig["Host"];
        
        if (string.IsNullOrEmpty(host))
        {
            _logger.LogInformation("Email Service is not configured. Fake sending email to {To}. Subject: {Subject}", to, subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailConfig["FromName"], emailConfig["FromAddress"]));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, int.Parse(emailConfig["Port"] ?? "587"), SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(emailConfig["Username"], emailConfig["Password"], cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }
}
