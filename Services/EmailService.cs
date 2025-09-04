using MailKit.Net.Smtp;
using MimeKit;
using UpgradeNotificationSystem.Models;

namespace UpgradeNotificationSystem.Services
{
    public interface IEmailService
    {
        Task SendUpgradeNotificationAsync(string toEmail, PlannedUpgrade upgrade);
        Task SendUpgradeConfirmationAsync(string toEmail, UpgradeJobPayload jobPayload);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendUpgradeNotificationAsync(string toEmail, PlannedUpgrade upgrade)
        {
            try
            {
                // Skip email in development if SMTP not configured
                var host = _configuration["Email:SmtpHost"];
                if (string.IsNullOrEmpty(host) || host == "smtp.gmail.com")
                {
                    _logger.LogInformation("Skipping email notification (development mode) for {Email}", toEmail);
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Upgrade Notification System", _configuration["Email:FromEmail"]));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = $"New System Upgrade Planned - Version {upgrade.Version}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = CreateUpgradeNotificationHtml(upgrade),
                    TextBody = CreateUpgradeNotificationText(upgrade)
                };

                message.Body = bodyBuilder.ToMessageBody();

                await SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send upgrade notification email to {Email}", toEmail);
                // Don't throw in development - just log the error
                if (!string.IsNullOrEmpty(_configuration["Email:Username"]))
                {
                    throw;
                }
            }
        }

        public async Task SendUpgradeConfirmationAsync(string toEmail, UpgradeJobPayload jobPayload)
        {
            try
            {
                // Skip email in development if SMTP not configured
                var host = _configuration["Email:SmtpHost"];
                if (string.IsNullOrEmpty(host) || host == "smtp.gmail.com")
                {
                    _logger.LogInformation("Skipping email notification (development mode) for {Email}", toEmail);
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Upgrade Notification System", _configuration["Email:FromEmail"]));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = $"Upgrade Scheduled - Version {jobPayload.Version}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = CreateUpgradeConfirmationHtml(jobPayload),
                    TextBody = CreateUpgradeConfirmationText(jobPayload)
                };

                message.Body = bodyBuilder.ToMessageBody();

                await SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send upgrade confirmation email to {Email}", toEmail);
                // Don't throw in development - just log the error
                if (!string.IsNullOrEmpty(_configuration["Email:Username"]))
                {
                    throw;
                }
            }
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            
            var host = _configuration["Email:SmtpHost"];
            var port = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var useSsl = bool.Parse(_configuration["Email:UseSsl"] ?? "true");

            await client.ConnectAsync(host, port, useSsl);
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private string CreateUpgradeNotificationHtml(PlannedUpgrade upgrade)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>System Upgrade Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .upgrade-info {{ background-color: #e3f2fd; padding: 20px; border-radius: 6px; margin: 20px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #2196f3; color: white; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚀 System Upgrade Notification</h1>
        </div>
        
        <p>Hello,</p>
        
        <p>A new system upgrade has been planned. Please review the details below:</p>
        
        <div class='upgrade-info'>
            <h3>Upgrade Details</h3>
            <p><strong>Version:</strong> {upgrade.Version}</p>
            <p><strong>Planned Date & Time:</strong> {upgrade.PlannedDateTime:yyyy-MM-dd HH:mm} UTC</p>
            {(string.IsNullOrEmpty(upgrade.Description) ? "" : $"<p><strong>Description:</strong> {upgrade.Description}</p>")}
        </div>
        
        <p>You can set your preferred upgrade time by logging into the application. If you don't set a preference, the upgrade will proceed at the planned time above.</p>
        
        <div class='footer'>
            <p>This is an automated message from the Upgrade Notification System.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string CreateUpgradeNotificationText(PlannedUpgrade upgrade)
        {
            return $@"
System Upgrade Notification

Hello,

A new system upgrade has been planned. Please review the details below:

Upgrade Details:
- Version: {upgrade.Version}
- Planned Date & Time: {upgrade.PlannedDateTime:yyyy-MM-dd HH:mm} UTC
{(string.IsNullOrEmpty(upgrade.Description) ? "" : $"- Description: {upgrade.Description}")}

You can set your preferred upgrade time by logging into the application. If you don't set a preference, the upgrade will proceed at the planned time above.

This is an automated message from the Upgrade Notification System.
";
        }

        private string CreateUpgradeConfirmationHtml(UpgradeJobPayload jobPayload)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Upgrade Confirmation</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .upgrade-info {{ background-color: #e8f5e8; padding: 20px; border-radius: 6px; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Upgrade Scheduled</h1>
        </div>
        
        <p>Hello,</p>
        
        <p>Your system upgrade has been scheduled successfully!</p>
        
        <div class='upgrade-info'>
            <h3>Scheduled Upgrade Details</h3>
            <p><strong>Version:</strong> {jobPayload.Version}</p>
            <p><strong>Scheduled Date & Time:</strong> {jobPayload.ScheduledDateTime:yyyy-MM-dd HH:mm} UTC</p>
        </div>
        
        <p>The upgrade will be automatically applied at the scheduled time. You will receive another notification when the upgrade is complete.</p>
        
        <div class='footer'>
            <p>This is an automated message from the Upgrade Notification System.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string CreateUpgradeConfirmationText(UpgradeJobPayload jobPayload)
        {
            return $@"
Upgrade Scheduled

Hello,

Your system upgrade has been scheduled successfully!

Scheduled Upgrade Details:
- Version: {jobPayload.Version}
- Scheduled Date & Time: {jobPayload.ScheduledDateTime:yyyy-MM-dd HH:mm} UTC

The upgrade will be automatically applied at the scheduled time. You will receive another notification when the upgrade is complete.

This is an automated message from the Upgrade Notification System.
";
        }
    }
}