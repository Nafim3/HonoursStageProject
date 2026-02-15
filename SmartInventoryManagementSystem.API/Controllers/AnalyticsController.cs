using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.Interfaces;

namespace SmartInventoryManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("chart")]
        public async Task<IActionResult> GetSalesAnalysis()
        {
            var result = await _analyticsService.GetSalesAnalysisAsync();
            return Ok(result);
        }
    }
}
