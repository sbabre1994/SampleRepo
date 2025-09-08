using System.ComponentModel.DataAnnotations;

namespace UpgradeNotificationSystem.Models
{
    public enum HistoryEventType
    {
        ReleaseScheduled,
        PreferenceSet,
        PreferenceUpdated,
        UpgradeExecuted,
        NotificationSent,
        JobScheduled
    }

    public class UpgradeHistory
    {
        public int Id { get; set; }
        
        [Required]
        public HistoryEventType EventType { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public int? PlannedReleaseId { get; set; }
        
        public int? UserPreferenceId { get; set; }
        
        public string? AdditionalData { get; set; } // JSON data for extra information
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual PlannedRelease? PlannedRelease { get; set; }
        public virtual UserPreference? UserPreference { get; set; }
    }
}