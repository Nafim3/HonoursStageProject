using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using System.Security.Claims;

namespace SmartInventoryManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IProductRepository _productRepo;
        private readonly ISaleRepository _saleRepo;
        private readonly IAlertRepository _alertRepo;

        public DashboardController(IProductRepository productRepo,ISaleRepository saleRepo, IAlertRepository alertRepository)
        {
            _productRepo = productRepo;
           
            _saleRepo = saleRepo;
            _alertRepo = alertRepository;
        }

        [HttpGet]
        public async Task<ActionResult<Dashboard>> GetDashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var totalProducts = (await _productRepo.GetProductAsync()).Count;

            var allSales = await _saleRepo.FetchAllSalesAsync();
            var salesToday = allSales.Count(s => s.SaleDate.Date == DateTime.UtcNow.Date);

            var lowStockCount = await _alertRepo.CountAsync(userId, "Low Stock");
            var expiredCount = await _alertRepo.CountAsync(userId, "Expired");

            var dto = new Dashboard
            {
                TotalProducts = totalProducts,
                SalesToday = salesToday,
                LowStockCount = lowStockCount,
                ExpiredProductsCount = expiredCount
            };

            return Ok(dto);
        }

    }
}
