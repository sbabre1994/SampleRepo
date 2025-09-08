using Hangfire;
using Microsoft.EntityFrameworkCore;
using UpgradeNotificationSystem.Data;
using UpgradeNotificationSystem.Models;

namespace UpgradeNotificationSystem.Services
{
    public interface IUpgradeJobService
    {
        Task ScheduleUpgradeJobsAsync(int plannedReleaseId);
        Task ExecuteUpgradeAsync(int plannedReleaseId, string? userId = null);
        Task CancelUpgradeJobsAsync(int plannedReleaseId);
    }

    public class UpgradeJobService : IUpgradeJobService
    {
        private readonly UpgradeContext _context;
        private readonly INotificationService _notificationService;
        private readonly IUpgradeManagementService _upgradeManagementService;
        private readonly ILogger<UpgradeJobService> _logger;

        public UpgradeJobService(
            UpgradeContext context,
            INotificationService notificationService,
            IUpgradeManagementService upgradeManagementService,
            ILogger<UpgradeJobService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _upgradeManagementService = upgradeManagementService;
            _logger = logger;
        }

        public async Task ScheduleUpgradeJobsAsync(int plannedReleaseId)
        {
            try
            {
                var release = await _context.PlannedReleases
                    .Include(r => r.UserPreferences)
                    .FirstOrDefaultAsync(r => r.Id == plannedReleaseId);

                if (release == null)
                {
                    _logger.LogWarning("Planned release {ReleaseId} not found for job scheduling", plannedReleaseId);
                    return;
                }

                // Schedule jobs for users with preferences
                foreach (var preference in release.UserPreferences)
                {
                    var executeTime = preference.UseDefaultTime ? release.PlannedDateTime : preference.PreferredDateTime;
                    
                    if (executeTime.HasValue && executeTime.Value > DateTime.UtcNow)
                    {
                        var jobId = BackgroundJob.Schedule(
                            () => ExecuteUpgradeAsync(plannedReleaseId, preference.UserId),
                            executeTime.Value);

                        _logger.LogInformation("Scheduled upgrade job {JobId} for user {UserId} at {Time}", 
                            jobId, preference.UserId, executeTime.Value);
                    }
                }

                // Schedule default job for any users without preferences
                var defaultJobId = BackgroundJob.Schedule(
                    () => ExecuteUpgradeAsync(plannedReleaseId, null),
                    release.PlannedDateTime);

                await _upgradeManagementService.LogHistoryAsync(
                    HistoryEventType.JobScheduled,
                    $"Upgrade jobs scheduled for version {release.Version}",
                    plannedReleaseId: plannedReleaseId,
                    additionalData: new { DefaultJobId = defaultJobId, ScheduledTime = release.PlannedDateTime });

                _logger.LogInformation("Scheduled upgrade jobs for release {Version} ({ReleaseId})", 
                    release.Version, plannedReleaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling upgrade jobs for release {ReleaseId}", plannedReleaseId);
                throw;
            }
        }

        public async Task ExecuteUpgradeAsync(int plannedReleaseId, string? userId = null)
        {
            try
            {
                var release = await _context.PlannedReleases
                    .FirstOrDefaultAsync(r => r.Id == plannedReleaseId);

                if (release == null)
                {
                    _logger.LogWarning("Planned release {ReleaseId} not found for execution", plannedReleaseId);
                    return;
                }

                var executionType = userId != null ? "individual" : "default";
                _logger.LogInformation("Executing {Type} upgrade for release {Version} (ID: {ReleaseId}), User: {UserId}", 
                    executionType, release.Version, plannedReleaseId, userId ?? "all");

                // Simulate upgrade execution
                await SimulateUpgradeProcessAsync(release, userId);

                // Log history
                await _upgradeManagementService.LogHistoryAsync(
                    HistoryEventType.UpgradeExecuted,
                    $"Upgrade executed for version {release.Version}" + (userId != null ? $" for user {userId}" : " (default)"),
                    userId,
                    plannedReleaseId,
                    additionalData: new { ExecutedAt = DateTime.UtcNow, ExecutionType = executionType });

                // Notify users
                var userIds = userId != null ? new List<string> { userId } : await GetAllUserIdsAsync();
                await _notificationService.NotifyUpgradeExecutedAsync(release, userIds);

                _logger.LogInformation("Completed {Type} upgrade execution for release {Version}", 
                    executionType, release.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing upgrade for release {ReleaseId}, user {UserId}", plannedReleaseId, userId);
                
                // Log error in history
                await _upgradeManagementService.LogHistoryAsync(
                    HistoryEventType.UpgradeExecuted,
                    $"Upgrade execution failed: {ex.Message}",
                    userId,
                    plannedReleaseId,
                    additionalData: new { Error = ex.Message, ExecutedAt = DateTime.UtcNow });
                
                throw;
            }
        }

        public async Task CancelUpgradeJobsAsync(int plannedReleaseId)
        {
            try
            {
                // In a real implementation, you would track job IDs and cancel them
                // For now, we'll just log the cancellation
                _logger.LogInformation("Cancelled upgrade jobs for release {ReleaseId}", plannedReleaseId);
                
                await _upgradeManagementService.LogHistoryAsync(
                    HistoryEventType.JobScheduled,
                    $"Upgrade jobs cancelled for release {plannedReleaseId}",
                    plannedReleaseId: plannedReleaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling upgrade jobs for release {ReleaseId}", plannedReleaseId);
                throw;
            }
        }

        private async Task SimulateUpgradeProcessAsync(PlannedRelease release, string? userId)
        {
            // Simulate upgrade steps
            _logger.LogInformation("Starting upgrade process for version {Version}...", release.Version);
            
            // Step 1: Backup
            await Task.Delay(1000);
            _logger.LogInformation("Backup completed for version {Version}", release.Version);
            
            // Step 2: Deploy
            await Task.Delay(2000);
            _logger.LogInformation("Deployment completed for version {Version}", release.Version);
            
            // Step 3: Verify
            await Task.Delay(500);
            _logger.LogInformation("Verification completed for version {Version}", release.Version);
            
            _logger.LogInformation("Upgrade process completed successfully for version {Version}", release.Version);
        }

        private async Task<List<string>> GetAllUserIdsAsync()
        {
            // In a real application, this would fetch from a user service
            // For demo purposes, we'll get unique user IDs from existing preferences
            var userIds = await _context.UserPreferences
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync();

            // Add some default demo user IDs if none exist
            if (!userIds.Any())
            {
                userIds = new List<string> { "admin", "user1", "user2" };
            }

            return userIds;
        }
    }
}