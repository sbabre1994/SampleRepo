using System.ComponentModel.DataAnnotations;

namespace UpgradeNotificationSystem.Models
{
    public class PlannedUpgrade
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Version { get; set; } = string.Empty;
        
        [Required]
        public DateTime PlannedDateTime { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public virtual ICollection<UserUpgradePreference> UserPreferences { get; set; } = new List<UserUpgradePreference>();
    }
}