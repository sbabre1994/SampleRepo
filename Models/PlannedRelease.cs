using System.ComponentModel.DataAnnotations;

namespace UpgradeNotificationSystem.Models
{
    public class PlannedRelease
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Version { get; set; } = string.Empty;
        
        [Required]
        public DateTime PlannedDateTime { get; set; }
        
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();
        public virtual ICollection<UpgradeHistory> UpgradeHistories { get; set; } = new List<UpgradeHistory>();
    }
}