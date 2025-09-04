using Microsoft.EntityFrameworkCore;
using UpgradeNotificationSystem.Models;

namespace UpgradeNotificationSystem.Data
{
    public class UpgradeNotificationContext : DbContext
    {
        public UpgradeNotificationContext(DbContextOptions<UpgradeNotificationContext> options)
            : base(options)
        {
        }

        public DbSet<PlannedUpgrade> PlannedUpgrades { get; set; }
        public DbSet<UserUpgradePreference> UserUpgradePreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PlannedUpgrade
            modelBuilder.Entity<PlannedUpgrade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PlannedDateTime).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
            });

            // Configure UserUpgradePreference
            modelBuilder.Entity<UserUpgradePreference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PlannedUpgradeId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Configure relationship
                entity.HasOne(e => e.PlannedUpgrade)
                      .WithMany(p => p.UserPreferences)
                      .HasForeignKey(e => e.PlannedUpgradeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Create unique index for user + upgrade combination
                entity.HasIndex(e => new { e.UserId, e.PlannedUpgradeId })
                      .IsUnique();
            });
        }
    }
}