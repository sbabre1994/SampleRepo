using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpgradeNotificationSystem.Data;
using UpgradeNotificationSystem.Models;
using UpgradeNotificationSystem.Services;

namespace UpgradeNotificationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly UpgradeNotificationContext _context;
        private readonly IUpgradeNotificationService _notificationService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UpgradeNotificationContext context,
            IUpgradeNotificationService notificationService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new planned upgrade and notify all users
        /// </summary>
        [HttpPost("upgrade/plan")]
        public async Task<ActionResult<PlannedUpgrade>> PlanUpgrade([FromBody] PlannedUpgrade upgrade)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Deactivate any existing active upgrades
                var existingActive = await _context.PlannedUpgrades
                    .Where(p => p.IsActive)
                    .ToListAsync();

                foreach (var existing in existingActive)
                {
                    existing.IsActive = false;
                }

                // Add the new upgrade
                upgrade.CreatedAt = DateTime.UtcNow;
                upgrade.IsActive = true;
                _context.PlannedUpgrades.Add(upgrade);
                
                await _context.SaveChangesAsync();

                // Notify all users
                await _notificationService.NotifyAllUsersOfPlannedUpgradeAsync(upgrade);

                _logger.LogInformation("Created new planned upgrade {Version} for {PlannedDateTime}", 
                    upgrade.Version, upgrade.PlannedDateTime);

                return CreatedAtAction(nameof(GetPlannedUpgrade), new { id = upgrade.Id }, upgrade);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating planned upgrade");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a specific planned upgrade by ID
        /// </summary>
        [HttpGet("upgrade/{id}")]
        public async Task<ActionResult<PlannedUpgrade>> GetPlannedUpgrade(int id)
        {
            try
            {
                var upgrade = await _context.PlannedUpgrades
                    .Include(p => p.UserPreferences)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (upgrade == null)
                {
                    return NotFound();
                }

                return Ok(upgrade);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting planned upgrade {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all planned upgrades
        /// </summary>
        [HttpGet("upgrades")]
        public async Task<ActionResult<IEnumerable<PlannedUpgrade>>> GetAllPlannedUpgrades()
        {
            try
            {
                var upgrades = await _context.PlannedUpgrades
                    .Include(p => p.UserPreferences)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return Ok(upgrades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all planned upgrades");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get user preferences for a specific planned upgrade
        /// </summary>
        [HttpGet("upgrade/{id}/preferences")]
        public async Task<ActionResult<IEnumerable<UserUpgradePreference>>> GetUpgradePreferences(int id)
        {
            try
            {
                var preferences = await _context.UserUpgradePreferences
                    .Include(p => p.PlannedUpgrade)
                    .Where(p => p.PlannedUpgradeId == id)
                    .ToListAsync();

                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preferences for upgrade {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cancel a planned upgrade
        /// </summary>
        [HttpPost("upgrade/{id}/cancel")]
        public async Task<ActionResult> CancelPlannedUpgrade(int id)
        {
            try
            {
                var upgrade = await _context.PlannedUpgrades.FindAsync(id);
                
                if (upgrade == null)
                {
                    return NotFound();
                }

                upgrade.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cancelled planned upgrade {Id} - {Version}", id, upgrade.Version);

                return Ok(new { message = "Planned upgrade cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling planned upgrade {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}