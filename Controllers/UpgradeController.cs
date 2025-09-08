using Microsoft.AspNetCore.Mvc;
using UpgradeNotificationSystem.DTOs;
using UpgradeNotificationSystem.Services;

namespace UpgradeNotificationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpgradeController : ControllerBase
    {
        private readonly IUpgradeManagementService _upgradeService;
        private readonly ILogger<UpgradeController> _logger;

        public UpgradeController(IUpgradeManagementService upgradeService, ILogger<UpgradeController> logger)
        {
            _upgradeService = upgradeService;
            _logger = logger;
        }

        /// <summary>
        /// Get the current active planned release
        /// </summary>
        [HttpGet("planned-release")]
        public async Task<ActionResult<PlannedReleaseDto>> GetPlannedRelease()
        {
            try
            {
                var release = await _upgradeService.GetActivePlannedReleaseAsync();
                if (release == null)
                {
                    return NotFound(new { message = "No active planned release found" });
                }

                return Ok(release);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting planned release");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new planned release (Admin only)
        /// </summary>
        [HttpPost("planned-release")]
        public async Task<ActionResult<PlannedReleaseDto>> CreatePlannedRelease([FromBody] CreatePlannedReleaseDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (dto.PlannedDateTime <= DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Planned date time must be in the future" });
                }

                var release = await _upgradeService.CreatePlannedReleaseAsync(dto);
                return Created($"/api/upgrade/planned-release", release);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating planned release");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user's upgrade preference for a specific release
        /// </summary>
        [HttpGet("preference/{userId}/{plannedReleaseId}")]
        public async Task<ActionResult<UserPreferenceDto>> GetUserPreference(string userId, int plannedReleaseId)
        {
            try
            {
                var preference = await _upgradeService.GetUserPreferenceAsync(userId, plannedReleaseId);
                if (preference == null)
                {
                    return NotFound(new { message = "User preference not found" });
                }

                return Ok(preference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preference for user {UserId} and release {ReleaseId}", userId, plannedReleaseId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Set or update user's upgrade preference
        /// </summary>
        [HttpPost("preference")]
        public async Task<ActionResult<UserPreferenceDto>> SetUserPreference([FromBody] SetUserPreferenceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!dto.UseDefaultTime && (!dto.PreferredDateTime.HasValue || dto.PreferredDateTime.Value <= DateTime.UtcNow))
                {
                    return BadRequest(new { message = "Preferred date time must be in the future when not using default time" });
                }

                var preference = await _upgradeService.SetUserPreferenceAsync(dto);
                return Ok(preference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user preference for user {UserId}", dto.UserId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get upgrade history with optional filtering
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<List<UpgradeHistoryDto>>> GetUpgradeHistory(
            [FromQuery] int? plannedReleaseId = null,
            [FromQuery] string? userId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var history = await _upgradeService.GetUpgradeHistoryAsync(plannedReleaseId, userId, page, pageSize);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upgrade history");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}