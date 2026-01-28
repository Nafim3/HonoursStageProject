using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.Interfaces;

namespace SmartInventoryManagementSystem.API.Controllers
{
    public class AlertController : ControllerBase
    {
        private readonly IAlertRepository _alertRepository;

        public AlertController(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentAlerts([FromQuery] string category, [FromQuery] int limit = 5)
        {
            var alerts = await _alertRepository.GetRecentAsync(category,limit);
            return Ok(alerts);
        }
    }
}
