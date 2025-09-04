using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UpgradeNotificationSystem.Data;
using UpgradeNotificationSystem.Hubs;
using UpgradeNotificationSystem.Models;

namespace UpgradeNotificationSystem.Services
{
    public interface IUpgradeNotificationService
    {
        Task NotifyAllUsersOfPlannedUpgradeAsync(PlannedUpgrade upgrade);
        Task NotifyUserOfScheduledUpgradeAsync(string userId, UpgradeJobPayload jobPayload);
        Task<PlannedUpgrade?> GetActivePlannedUpgradeAsync();
        Task<UserUpgradePreference?> GetUserPreferenceAsync(string userId, int plannedUpgradeId);
        Task<UserUpgradePreference> SetUserPreferenceAsync(SetPreferenceRequest request);
    }

    public class UpgradeNotificationService : IUpgradeNotificationService
    {
        private readonly UpgradeNotificationContext _context;
        private readonly IHubContext<UpgradeNotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly IUpgradeJobService _jobService;
        private readonly ILogger<UpgradeNotificationService> _logger;

        public UpgradeNotificationService(
            UpgradeNotificationContext context,
            IHubContext<UpgradeNotificationHub> hubContext,
            IEmailService emailService,
            IUpgradeJobService jobService,
            ILogger<UpgradeNotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
            _jobService = jobService;
            _logger = logger;
        }

        public async Task NotifyAllUsersOfPlannedUpgradeAsync(PlannedUpgrade upgrade)
        {
            try
            {
                // Send SignalR notification to all connected clients
                await _hubContext.Clients.All.SendAsync("UpgradePlanned", new
                {
                    Id = upgrade.Id,
                    Version = upgrade.Version,
                    PlannedDateTime = upgrade.PlannedDateTime,
                    Description = upgrade.Description
                });

                _logger.LogInformation("Sent SignalR notification for planned upgrade {Version}", upgrade.Version);

                // For demonstration, we'll send email to a configured list of users
                // In a real application, you would query your user database
                var userEmails = GetAllUserEmails(); // This would be from your user management system
                
                foreach (var email in userEmails)
                {
                    try
                    {
                        await _emailService.SendUpgradeNotificationAsync(email, upgrade);
                        _logger.LogInformation("Sent email notification to {Email} for upgrade {Version}", email, upgrade.Version);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email notification to {Email}", email);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify users of planned upgrade {Version}", upgrade.Version);
                throw;
            }
        }

        public async Task NotifyUserOfScheduledUpgradeAsync(string userId, UpgradeJobPayload jobPayload)
        {
            try
            {
                // Send SignalR notification to specific user
                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("UpgradeScheduled", jobPayload);

                // Send email confirmation
                await _emailService.SendUpgradeConfirmationAsync(jobPayload.UserEmail, jobPayload);

                _logger.LogInformation("Notified user {UserId} of scheduled upgrade {Version}", userId, jobPayload.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify user {UserId} of scheduled upgrade", userId);
                throw;
            }
        }

        public async Task<PlannedUpgrade?> GetActivePlannedUpgradeAsync()
        {
            return await _context.PlannedUpgrades
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<UserUpgradePreference?> GetUserPreferenceAsync(string userId, int plannedUpgradeId)
        {
            return await _context.UserUpgradePreferences
                .Include(p => p.PlannedUpgrade)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.PlannedUpgradeId == plannedUpgradeId);
        }

        public async Task<UserUpgradePreference> SetUserPreferenceAsync(SetPreferenceRequest request)
        {
            var existingPreference = await _context.UserUpgradePreferences
                .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.PlannedUpgradeId == request.PlannedUpgradeId);

            var plannedUpgrade = await _context.PlannedUpgrades
                .FirstOrDefaultAsync(p => p.Id == request.PlannedUpgradeId);

            if (plannedUpgrade == null)
            {
                throw new ArgumentException("Planned upgrade not found");
            }

            if (existingPreference != null)
            {
                // Update existing preference
                existingPreference.PreferredDateTime = request.PreferredDateTime;
                existingPreference.UpdatedAt = DateTime.UtcNow;
                existingPreference.JobScheduled = false; // Reset job scheduling flag
            }
            else
            {
                // Create new preference
                existingPreference = new UserUpgradePreference
                {
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    PlannedUpgradeId = request.PlannedUpgradeId,
                    PreferredDateTime = request.PreferredDateTime,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserUpgradePreferences.Add(existingPreference);
            }

            await _context.SaveChangesAsync();

            // Schedule the upgrade job
            var jobPayload = new UpgradeJobPayload
            {
                PlannedUpgradeId = plannedUpgrade.Id,
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                Version = plannedUpgrade.Version,
                ScheduledDateTime = request.PreferredDateTime ?? plannedUpgrade.PlannedDateTime
            };

            await _jobService.ScheduleUpgradeJobAsync(jobPayload);
            
            existingPreference.JobScheduled = true;
            await _context.SaveChangesAsync();

            // Notify user of scheduled upgrade
            await NotifyUserOfScheduledUpgradeAsync(request.UserId, jobPayload);

            return existingPreference;
        }

        private IEnumerable<string> GetAllUserEmails()
        {
            // This is a placeholder. In a real application, you would query your user management system
            // For demonstration purposes, we'll return some sample emails
            var sampleEmails = new List<string>
            {
                "user1@example.com",
                "user2@example.com",
                "admin@example.com"
            };

            // You could also read from configuration
            var configuredEmails = Environment.GetEnvironmentVariable("DEMO_USER_EMAILS")?.Split(',')
                ?? Array.Empty<string>();

            return sampleEmails.Concat(configuredEmails.Where(e => !string.IsNullOrWhiteSpace(e)));
        }
    }
}