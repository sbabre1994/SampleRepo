using System.ComponentModel.DataAnnotations;

namespace UpgradeNotificationSystem.Models
{
    public class UserUpgradePreference
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string UserEmail { get; set; } = string.Empty;
        
        [Required]
        public int PlannedUpgradeId { get; set; }
        
        public DateTime? PreferredDateTime { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool EmailNotificationSent { get; set; } = false;
        
        public bool JobScheduled { get; set; } = false;
        
        // Navigation property
        public virtual PlannedUpgrade PlannedUpgrade { get; set; } = null!;
    }
}