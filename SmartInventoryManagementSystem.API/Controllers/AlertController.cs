using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO.NotificationDTO;
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
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;
        public AlertController(IAlertRepository alertrepository, INotificationService notificationService, IProductRepository productRepo, ICurrentUserService currentUserService)
        {
            _alertRepository = alertrepository;
            _notificationService = notificationService;
            _productRepository = productRepo;
            _currentUserService = currentUserService;
        }


        [HttpGet("bell")]
        public async Task<IActionResult> GetBellAlerts()
        {
            //var userId = int.Parse(User.FindFirstValue (ClaimTypes.NameIdentifier)!);

            //var alerts = await _alertRepository.GetAllForUserAsync(userId);

            //var dto = alerts.Select(a => new AlertDTO
            //{
            //    Id = a.Id,
            //    ProductId = a.ProductId,
            //    Category = a.Category,
            //    Message = a.Message,
            //    CreatedAt = a.CreatedAt
            //}).ToList();

            //return Ok(dto);

            var userId = _currentUserService.UserId;

            var alerts = await _notificationService.GetBellAlertsAsync(userId);

            return Ok(alerts);


        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllAlerts()
        {
            var userId = _currentUserService.UserId;

            var alerts = await _alertRepository.GetAllForUserAsync(userId);

            var dto = alerts.Select(a => new AlertDTO
            {
                Id = a.Id,
                ProductId = a.ProductId,
                Category = a.Category,
                Message = a.Message,
                CreatedAt = a.CreatedAt
            }).ToList();

            return Ok(dto);
        }


        [HttpDelete("{alertId}")]
        public async Task<IActionResult> DeleteAlert(int alertId)
        {
            var userId = _currentUserService.UserId;

            var alert = await _alertRepository.GetByIdAsync(alertId);

            if (alert == null || alert.UserId != userId)
                return NotFound();

            await _alertRepository.DeleteAsync(alert);

            return Ok();
        }



        [HttpGet("refresh")]
        public async Task<IActionResult> RefreshAlerts()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var alerts = await _notificationService.GetBellAlertsAsync(userId);
            return Ok(alerts);
        }
    }
}
