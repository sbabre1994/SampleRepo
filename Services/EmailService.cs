namespace UpgradeNotificationSystem.Services
{
    public interface IEmailService
    {
        Task SendUpgradeNotificationAsync(string toEmail, string userName, string version, DateTime plannedDateTime);
        Task SendPreferenceConfirmationAsync(string toEmail, string userName, string version, DateTime? preferredDateTime, bool useDefaultTime);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendUpgradeNotificationAsync(string toEmail, string userName, string version, DateTime plannedDateTime)
        {
            try
            {
                var subject = $"System Upgrade Scheduled - Version {version}";
                var body = GetUpgradeNotificationTemplate(userName, version, plannedDateTime);
                
                // In a real implementation, you would use a service like SendGrid, SMTP, or AWS SES
                // For now, we'll just log the email content
                _logger.LogInformation("Sending upgrade notification email to {Email}: {Subject}", toEmail, subject);
                _logger.LogInformation("Email body: {Body}", body);
                
                // Simulate email sending delay
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send upgrade notification email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendPreferenceConfirmationAsync(string toEmail, string userName, string version, DateTime? preferredDateTime, bool useDefaultTime)
        {
            try
            {
                var subject = $"Upgrade Preference Confirmed - Version {version}";
                var body = GetPreferenceConfirmationTemplate(userName, version, preferredDateTime, useDefaultTime);
                
                _logger.LogInformation("Sending preference confirmation email to {Email}: {Subject}", toEmail, subject);
                _logger.LogInformation("Email body: {Body}", body);
                
                // Simulate email sending delay
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send preference confirmation email to {Email}", toEmail);
                throw;
            }
        }

        private string GetUpgradeNotificationTemplate(string userName, string version, DateTime plannedDateTime)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>System Upgrade Notification</h1>
        </div>
        <div class=""content"">
            <h2>Hello {userName},</h2>
            <p>We have scheduled a system upgrade that will improve your experience with our application.</p>
            
            <h3>Upgrade Details:</h3>
            <ul>
                <li><strong>Version:</strong> {version}</li>
                <li><strong>Scheduled Date & Time:</strong> {plannedDateTime:yyyy-MM-dd HH:mm:ss UTC}</li>
            </ul>
            
            <p>You can set your preferred upgrade time by logging into your account and visiting the upgrade preferences page. If you don't set a preference, the upgrade will proceed at the scheduled time above.</p>
            
            <p>The system will be temporarily unavailable during the upgrade process.</p>
            
            <div class=""footer"">
                <p>This is an automated notification. Please do not reply to this email.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPreferenceConfirmationTemplate(string userName, string version, DateTime? preferredDateTime, bool useDefaultTime)
        {
            var preferenceText = useDefaultTime 
                ? "You have chosen to use the default scheduled time." 
                : $"Your preferred upgrade time: {preferredDateTime:yyyy-MM-dd HH:mm:ss UTC}";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Upgrade Preference Confirmed</h1>
        </div>
        <div class=""content"">
            <h2>Hello {userName},</h2>
            <p>We have received and confirmed your upgrade preference for version {version}.</p>
            
            <h3>Your Preference:</h3>
            <p>{preferenceText}</p>
            
            <p>You can update your preference at any time before the upgrade is executed by visiting your account preferences.</p>
            
            <div class=""footer"">
                <p>This is an automated notification. Please do not reply to this email.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}