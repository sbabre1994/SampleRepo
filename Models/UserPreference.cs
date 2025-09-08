using System.ComponentModel.DataAnnotations;

namespace UpgradeNotificationSystem.Models
{
    public class UserPreference
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string UserEmail { get; set; } = string.Empty;
        
        [Required]
        public int PlannedReleaseId { get; set; }
        
        public DateTime? PreferredDateTime { get; set; }
        
        public bool UseDefaultTime { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual PlannedRelease PlannedRelease { get; set; } = null!;
        public virtual ICollection<UpgradeHistory> UpgradeHistories { get; set; } = new List<UpgradeHistory>();
    }
}