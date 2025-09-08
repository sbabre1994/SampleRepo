using Microsoft.EntityFrameworkCore;
using UpgradeNotificationSystem.Models;

namespace UpgradeNotificationSystem.Data
{
    public class UpgradeContext : DbContext
    {
        public UpgradeContext(DbContextOptions<UpgradeContext> options) : base(options)
        {
        }

        public DbSet<PlannedRelease> PlannedReleases { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<UpgradeHistory> UpgradeHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PlannedRelease
            modelBuilder.Entity<PlannedRelease>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Version).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PlannedDateTime).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => e.Version);
                entity.HasIndex(e => e.PlannedDateTime);
            });

            // Configure UserPreference
            modelBuilder.Entity<UserPreference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(255).IsRequired();
                entity.Property(e => e.UserEmail).HasMaxLength(255).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Foreign key relationship
                entity.HasOne(e => e.PlannedRelease)
                      .WithMany(e => e.UserPreferences)
                      .HasForeignKey(e => e.PlannedReleaseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint for user and planned release
                entity.HasIndex(e => new { e.UserId, e.PlannedReleaseId }).IsUnique();
                entity.HasIndex(e => e.UserEmail);
            });

            // Configure UpgradeHistory
            modelBuilder.Entity<UpgradeHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).HasConversion<string>().IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Foreign key relationships
                entity.HasOne(e => e.PlannedRelease)
                      .WithMany(e => e.UpgradeHistories)
                      .HasForeignKey(e => e.PlannedReleaseId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.UserPreference)
                      .WithMany(e => e.UpgradeHistories)
                      .HasForeignKey(e => e.UserPreferenceId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
            });
        }
    }
}