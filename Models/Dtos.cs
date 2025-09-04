namespace UpgradeNotificationSystem.Models
{
    public class UpgradeNotificationDto
    {
        public int PlannedUpgradeId { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime PlannedDateTime { get; set; }
        public string? Description { get; set; }
        public DateTime? UserPreferredDateTime { get; set; }
        public bool HasUserPreference { get; set; }
    }

    public class SetPreferenceRequest
    {
        public int PlannedUpgradeId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime? PreferredDateTime { get; set; }
    }

    public class UpgradeJobPayload
    {
        public int PlannedUpgradeId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime ScheduledDateTime { get; set; }
    }
}