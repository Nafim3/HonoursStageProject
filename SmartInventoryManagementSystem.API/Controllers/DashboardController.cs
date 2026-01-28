using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;

namespace SmartInventoryManagementSystem.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IProductRepository _productRepo;
        private readonly ISaleRepository _saleRepo;

        public DashboardController(IProductRepository productRepo,
                                   
                                   ISaleRepository saleRepo)
        {
            _productRepo = productRepo;
           
            _saleRepo = saleRepo;
        }

        [HttpGet]
        public async Task<ActionResult<Dashboard>> GetDashboard()
        {
            var totalProducts = (await _productRepo.GetProductAsync()).Count;

            var allSales = await _saleRepo.FetchAllSalesAsync();
            var salesToday = allSales.Count(s => s.SaleDate.Date == DateTime.UtcNow.Date);

            var lowStockCount = (await _productRepo.GetLowStockProductsAsync()).Count;

            var dto = new Dashboard
            {
                TotalProducts = totalProducts,
                SalesToday = salesToday,
                LowStockCount = lowStockCount
            };

            return Ok(dto);
        }

    }
}
