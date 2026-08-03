using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuttorLib.Core.Interfaces;
using SuttorLibrary.DTOs;
using System.Security.Claims;

namespace SuttorLib.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController(IFCM fcmService, ILogger<NotificationsController> logger) : ControllerBase
    {
        private readonly IFCM _fcmService = fcmService;
        private readonly ILogger<NotificationsController> _logger = logger;

        [HttpPost("register")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterFCMTokenDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _fcmService.RegisterTokenAsync(userId, dto.Token, dto.DeviceName, dto.Platform);
                return Ok(new { success = true, message = "Token registered" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering FCM token for user {UserId}", userId);
                return Problem("An error occurred while registering the token.");
            }
        }

        [HttpPost("unregister")]
        public async Task<IActionResult> UnregisterToken([FromBody] string token)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _fcmService.UnregisterTokenAsync(userId, token);
                return Ok(new { success = true, message = "Token unregistered" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering FCM token for user {UserId}", userId);
                return Problem("An error occurred while unregistering the token.");
            }
        }

        [HttpPost("send-test")]
        public async Task<IActionResult> SendTest([FromBody] SendNotificationDto dto)
        {
            try
            {
                await _fcmService.SendToUserAsync(dto.UserId, dto.Title, dto.Body, dto.Data);
                return Ok(new { success = true, message = "Notification sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification to user {UserId}", dto.UserId);
                return Problem("An error occurred while sending the notification.");
            }
        }

        [HttpGet("Notification-Log")]
        public async Task<IActionResult> NotificationLog([FromBody] string userId)
        {
            try {
                if (userId is null)
                    return BadRequest("User ID is required");

                var log = await _fcmService.NotificationLog(userId);
                if (log is null || log.Count == 0)
                    return NotFound("No notification for you");

                _logger.LogInformation("Getting notifications for user {userId} successfully", userId);
                return Ok(log);
            
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {userId}", userId);
                return Problem("An error occurred while getting notifications");
            }
        }
    }
}
