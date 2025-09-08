using Microsoft.AspNetCore.SignalR;
using UpgradeNotificationSystem.Hubs;
using UpgradeNotificationSystem.Models;

namespace UpgradeNotificationSystem.Services
{
    public interface INotificationService
    {
        Task NotifyNewUpgradeScheduledAsync(PlannedRelease release, List<string> userEmails);
        Task NotifyPreferenceUpdatedAsync(string userId, string userEmail, UserPreference preference);
        Task NotifyUpgradeExecutedAsync(PlannedRelease release, List<string> userIds);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<UpgradeNotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<UpgradeNotificationHub> hubContext,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task NotifyNewUpgradeScheduledAsync(PlannedRelease release, List<string> userEmails)
        {
            try
            {
                // Send SignalR notification to all connected clients
                await _hubContext.Clients.All.SendAsync("NewUpgradeScheduled", new
                {
                    Version = release.Version,
                    PlannedDateTime = release.PlannedDateTime,
                    Description = release.Description,
                    ReleaseId = release.Id
                });

                // Send email notifications
                var emailTasks = userEmails.Select(async email =>
                {
                    try
                    {
                        await _emailService.SendUpgradeNotificationAsync(
                            email, 
                            ExtractUserNameFromEmail(email), 
                            release.Version, 
                            release.PlannedDateTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email notification to {Email}", email);
                    }
                });

                await Task.WhenAll(emailTasks);

                _logger.LogInformation("Sent upgrade notifications for release {Version} to {Count} users", 
                    release.Version, userEmails.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send upgrade notifications for release {Version}", release.Version);
                throw;
            }
        }

        public async Task NotifyPreferenceUpdatedAsync(string userId, string userEmail, UserPreference preference)
        {
            try
            {
                // Send SignalR notification to specific user
                await _hubContext.Clients.Group($"User-{userId}").SendAsync("PreferenceUpdated", new
                {
                    PreferenceId = preference.Id,
                    PlannedReleaseId = preference.PlannedReleaseId,
                    PreferredDateTime = preference.PreferredDateTime,
                    UseDefaultTime = preference.UseDefaultTime,
                    UpdatedAt = preference.UpdatedAt
                });

                // Send confirmation email
                await _emailService.SendPreferenceConfirmationAsync(
                    userEmail,
                    ExtractUserNameFromEmail(userEmail),
                    preference.PlannedRelease?.Version ?? "Unknown",
                    preference.PreferredDateTime,
                    preference.UseDefaultTime);

                _logger.LogInformation("Sent preference confirmation to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send preference confirmation to user {UserId}", userId);
                throw;
            }
        }

        public async Task NotifyUpgradeExecutedAsync(PlannedRelease release, List<string> userIds)
        {
            try
            {
                // Send SignalR notification to all specified users
                var notificationTasks = userIds.Select(async userId =>
                {
                    try
                    {
                        await _hubContext.Clients.Group($"User-{userId}").SendAsync("UpgradeExecuted", new
                        {
                            Version = release.Version,
                            ExecutedAt = DateTime.UtcNow,
                            ReleaseId = release.Id
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send SignalR notification to user {UserId}", userId);
                    }
                });

                await Task.WhenAll(notificationTasks);

                _logger.LogInformation("Sent upgrade execution notifications for release {Version} to {Count} users", 
                    release.Version, userIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send upgrade execution notifications for release {Version}", release.Version);
                throw;
            }
        }

        private string ExtractUserNameFromEmail(string email)
        {
            // Simple extraction - take part before @ symbol
            var atIndex = email.IndexOf('@');
            return atIndex > 0 ? email.Substring(0, atIndex) : email;
        }
    }
}