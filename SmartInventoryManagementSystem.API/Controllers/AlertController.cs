using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.Interfaces;
using System.Security.Claims;

namespace SmartInventoryManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class AlertController : ControllerBase
    {
        private readonly IAlertRepository _alertRepository;
        private readonly INotificationService _notificationService;
        public AlertController(IAlertRepository alertrepository, INotificationService notificationService)
        {
            _alertRepository = alertrepository;
            _notificationService = notificationService;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentAlerts([FromQuery] string category, [FromQuery] int limit = 5)
        {
            
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var alerts = await _alertRepository.GetRecentAsync(userId, category, limit);
                return Ok(alerts);
            
        }

        [HttpGet("bell")]
        public async Task<IActionResult> GetBellAlerts()
        {
            var userId = int.Parse(User.FindFirstValue (ClaimTypes.NameIdentifier)!);

            return Ok(await _notificationService.GetBellAlertsAsync(userId));
        }
    }
}
