namespace UpgradeNotificationSystem.DTOs
{
    public class PlannedReleaseDto
    {
        public int Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime PlannedDateTime { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePlannedReleaseDto
    {
        public string Version { get; set; } = string.Empty;
        public DateTime PlannedDateTime { get; set; }
        public string? Description { get; set; }
    }

    public class UserPreferenceDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int PlannedReleaseId { get; set; }
        public DateTime? PreferredDateTime { get; set; }
        public bool UseDefaultTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PlannedReleaseDto? PlannedRelease { get; set; }
    }

    public class SetUserPreferenceDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int PlannedReleaseId { get; set; }
        public DateTime? PreferredDateTime { get; set; }
        public bool UseDefaultTime { get; set; } = true;
    }

    public class UpgradeHistoryDto
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public int? PlannedReleaseId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? AdditionalData { get; set; }
        public PlannedReleaseDto? PlannedRelease { get; set; }
    }
}