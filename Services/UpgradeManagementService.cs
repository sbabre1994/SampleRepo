using Microsoft.EntityFrameworkCore;
using UpgradeNotificationSystem.Data;
using UpgradeNotificationSystem.DTOs;
using UpgradeNotificationSystem.Models;
using System.Text.Json;

namespace UpgradeNotificationSystem.Services
{
    public interface IUpgradeManagementService
    {
        Task<PlannedReleaseDto?> GetActivePlannedReleaseAsync();
        Task<PlannedReleaseDto> CreatePlannedReleaseAsync(CreatePlannedReleaseDto dto);
        Task<UserPreferenceDto?> GetUserPreferenceAsync(string userId, int plannedReleaseId);
        Task<UserPreferenceDto> SetUserPreferenceAsync(SetUserPreferenceDto dto);
        Task<List<UpgradeHistoryDto>> GetUpgradeHistoryAsync(int? plannedReleaseId = null, string? userId = null, int page = 1, int pageSize = 50);
        Task LogHistoryAsync(HistoryEventType eventType, string description, string? userId = null, int? plannedReleaseId = null, int? userPreferenceId = null, object? additionalData = null);
    }

    public class UpgradeManagementService : IUpgradeManagementService
    {
        private readonly UpgradeContext _context;
        private readonly INotificationService _notificationService;
        private readonly IS3SyncService _s3SyncService;
        private readonly ILogger<UpgradeManagementService> _logger;

        public UpgradeManagementService(
            UpgradeContext context,
            INotificationService notificationService,
            IS3SyncService s3SyncService,
            ILogger<UpgradeManagementService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _s3SyncService = s3SyncService;
            _logger = logger;
        }

        public async Task<PlannedReleaseDto?> GetActivePlannedReleaseAsync()
        {
            var release = await _context.PlannedReleases
                .Where(r => r.IsActive && r.PlannedDateTime > DateTime.UtcNow)
                .OrderBy(r => r.PlannedDateTime)
                .FirstOrDefaultAsync();

            return release != null ? MapToDto(release) : null;
        }

        public async Task<PlannedReleaseDto> CreatePlannedReleaseAsync(CreatePlannedReleaseDto dto)
        {
            var release = new PlannedRelease
            {
                Version = dto.Version,
                PlannedDateTime = dto.PlannedDateTime,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.PlannedReleases.Add(release);
            await _context.SaveChangesAsync();

            // Log history
            await LogHistoryAsync(HistoryEventType.ReleaseScheduled, 
                $"New upgrade scheduled for version {release.Version}", 
                plannedReleaseId: release.Id);

            // Sync to S3
            await _s3SyncService.SyncPlannedReleaseAsync(release, $"{release.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.json");

            // Get all user emails for notifications (in a real app, this would come from a user service)
            var userEmails = await GetAllUserEmailsAsync();
            
            // Send notifications
            await _notificationService.NotifyNewUpgradeScheduledAsync(release, userEmails);

            _logger.LogInformation("Created planned release {Version} with ID {Id}", release.Version, release.Id);

            return MapToDto(release);
        }

        public async Task<UserPreferenceDto?> GetUserPreferenceAsync(string userId, int plannedReleaseId)
        {
            var preference = await _context.UserPreferences
                .Include(p => p.PlannedRelease)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.PlannedReleaseId == plannedReleaseId);

            return preference != null ? MapToDto(preference) : null;
        }

        public async Task<UserPreferenceDto> SetUserPreferenceAsync(SetUserPreferenceDto dto)
        {
            var existingPreference = await _context.UserPreferences
                .Include(p => p.PlannedRelease)
                .FirstOrDefaultAsync(p => p.UserId == dto.UserId && p.PlannedReleaseId == dto.PlannedReleaseId);

            UserPreference preference;
            bool isUpdate = existingPreference != null;

            if (existingPreference != null)
            {
                // Update existing preference
                existingPreference.PreferredDateTime = dto.PreferredDateTime;
                existingPreference.UseDefaultTime = dto.UseDefaultTime;
                existingPreference.UserEmail = dto.UserEmail;
                existingPreference.UpdatedAt = DateTime.UtcNow;
                preference = existingPreference;
            }
            else
            {
                // Create new preference
                preference = new UserPreference
                {
                    UserId = dto.UserId,
                    UserEmail = dto.UserEmail,
                    PlannedReleaseId = dto.PlannedReleaseId,
                    PreferredDateTime = dto.PreferredDateTime,
                    UseDefaultTime = dto.UseDefaultTime,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserPreferences.Add(preference);
            }

            await _context.SaveChangesAsync();

            // Reload with related data
            await _context.Entry(preference).Reference(p => p.PlannedRelease).LoadAsync();

            // Log history
            await LogHistoryAsync(
                isUpdate ? HistoryEventType.PreferenceUpdated : HistoryEventType.PreferenceSet,
                $"User preference {(isUpdate ? "updated" : "set")} for version {preference.PlannedRelease?.Version}",
                dto.UserId,
                dto.PlannedReleaseId,
                preference.Id);

            // Sync to S3
            await _s3SyncService.SyncUserPreferenceAsync(preference, $"{preference.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.json");

            // Send notifications
            await _notificationService.NotifyPreferenceUpdatedAsync(dto.UserId, dto.UserEmail, preference);

            _logger.LogInformation("User {UserId} {Action} preference for release {ReleaseId}", 
                dto.UserId, isUpdate ? "updated" : "set", dto.PlannedReleaseId);

            return MapToDto(preference);
        }

        public async Task<List<UpgradeHistoryDto>> GetUpgradeHistoryAsync(int? plannedReleaseId = null, string? userId = null, int page = 1, int pageSize = 50)
        {
            var query = _context.UpgradeHistories
                .Include(h => h.PlannedRelease)
                .AsQueryable();

            if (plannedReleaseId.HasValue)
                query = query.Where(h => h.PlannedReleaseId == plannedReleaseId.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(h => h.UserId == userId);

            var histories = await query
                .OrderByDescending(h => h.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return histories.Select(MapToDto).ToList();
        }

        public async Task LogHistoryAsync(HistoryEventType eventType, string description, string? userId = null, int? plannedReleaseId = null, int? userPreferenceId = null, object? additionalData = null)
        {
            var history = new UpgradeHistory
            {
                EventType = eventType,
                Description = description,
                UserId = userId,
                PlannedReleaseId = plannedReleaseId,
                UserPreferenceId = userPreferenceId,
                AdditionalData = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
                Timestamp = DateTime.UtcNow
            };

            _context.UpgradeHistories.Add(history);
            await _context.SaveChangesAsync();

            // Sync to S3
            await _s3SyncService.SyncUpgradeHistoryAsync(history, $"{history.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
        }

        private async Task<List<string>> GetAllUserEmailsAsync()
        {
            // In a real application, this would fetch from a user service or user table
            // For demo purposes, we'll get unique emails from existing preferences
            var emails = await _context.UserPreferences
                .Select(p => p.UserEmail)
                .Distinct()
                .ToListAsync();

            // Add some default demo emails if none exist
            if (!emails.Any())
            {
                emails = new List<string> { "admin@example.com", "user1@example.com", "user2@example.com" };
            }

            return emails;
        }

        private PlannedReleaseDto MapToDto(PlannedRelease release)
        {
            return new PlannedReleaseDto
            {
                Id = release.Id,
                Version = release.Version,
                PlannedDateTime = release.PlannedDateTime,
                Description = release.Description,
                CreatedAt = release.CreatedAt,
                IsActive = release.IsActive
            };
        }

        private UserPreferenceDto MapToDto(UserPreference preference)
        {
            return new UserPreferenceDto
            {
                Id = preference.Id,
                UserId = preference.UserId,
                UserEmail = preference.UserEmail,
                PlannedReleaseId = preference.PlannedReleaseId,
                PreferredDateTime = preference.PreferredDateTime,
                UseDefaultTime = preference.UseDefaultTime,
                CreatedAt = preference.CreatedAt,
                UpdatedAt = preference.UpdatedAt,
                PlannedRelease = preference.PlannedRelease != null ? MapToDto(preference.PlannedRelease) : null
            };
        }

        private UpgradeHistoryDto MapToDto(UpgradeHistory history)
        {
            return new UpgradeHistoryDto
            {
                Id = history.Id,
                EventType = history.EventType.ToString(),
                Description = history.Description,
                UserId = history.UserId,
                PlannedReleaseId = history.PlannedReleaseId,
                Timestamp = history.Timestamp,
                AdditionalData = history.AdditionalData,
                PlannedRelease = history.PlannedRelease != null ? MapToDto(history.PlannedRelease) : null
            };
        }
    }
}