using Microsoft.AspNetCore.Mvc;
using UpgradeNotificationSystem.Models;
using UpgradeNotificationSystem.Services;

namespace UpgradeNotificationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpgradeController : ControllerBase
    {
        private readonly IUpgradeNotificationService _notificationService;
        private readonly ILogger<UpgradeController> _logger;

        public UpgradeController(IUpgradeNotificationService notificationService, ILogger<UpgradeController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get the current active planned upgrade
        /// </summary>
        [HttpGet("planned")]
        public async Task<ActionResult<UpgradeNotificationDto>> GetPlannedUpgrade()
        {
            try
            {
                var plannedUpgrade = await _notificationService.GetActivePlannedUpgradeAsync();
                
                if (plannedUpgrade == null)
                {
                    return NotFound("No active planned upgrade found");
                }

                var result = new UpgradeNotificationDto
                {
                    PlannedUpgradeId = plannedUpgrade.Id,
                    Version = plannedUpgrade.Version,
                    PlannedDateTime = plannedUpgrade.PlannedDateTime,
                    Description = plannedUpgrade.Description,
                    HasUserPreference = false
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting planned upgrade");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a user's upgrade preference for a specific planned upgrade
        /// </summary>
        [HttpGet("preference/{userId}/{plannedUpgradeId}")]
        public async Task<ActionResult<UpgradeNotificationDto>> GetUserPreference(string userId, int plannedUpgradeId)
        {
            try
            {
                var preference = await _notificationService.GetUserPreferenceAsync(userId, plannedUpgradeId);
                
                if (preference?.PlannedUpgrade == null)
                {
                    return NotFound("User preference not found");
                }

                var result = new UpgradeNotificationDto
                {
                    PlannedUpgradeId = preference.PlannedUpgrade.Id,
                    Version = preference.PlannedUpgrade.Version,
                    PlannedDateTime = preference.PlannedUpgrade.PlannedDateTime,
                    Description = preference.PlannedUpgrade.Description,
                    UserPreferredDateTime = preference.PreferredDateTime,
                    HasUserPreference = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preference for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Set or update a user's upgrade preference
        /// </summary>
        [HttpPost("preference")]
        public async Task<ActionResult<UserUpgradePreference>> SetUserPreference([FromBody] SetPreferenceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var preference = await _notificationService.SetUserPreferenceAsync(request);
                return Ok(preference);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for setting user preference");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user preference for user {UserId}", request.UserId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get user preference combined with planned upgrade info
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UpgradeNotificationDto>> GetUserUpgradeInfo(string userId)
        {
            try
            {
                var plannedUpgrade = await _notificationService.GetActivePlannedUpgradeAsync();
                
                if (plannedUpgrade == null)
                {
                    return NotFound("No active planned upgrade found");
                }

                var preference = await _notificationService.GetUserPreferenceAsync(userId, plannedUpgrade.Id);

                var result = new UpgradeNotificationDto
                {
                    PlannedUpgradeId = plannedUpgrade.Id,
                    Version = plannedUpgrade.Version,
                    PlannedDateTime = plannedUpgrade.PlannedDateTime,
                    Description = plannedUpgrade.Description,
                    UserPreferredDateTime = preference?.PreferredDateTime,
                    HasUserPreference = preference != null
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user upgrade info for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}