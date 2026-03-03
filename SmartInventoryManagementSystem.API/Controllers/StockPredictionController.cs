using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Infrastructure.Services;

namespace SmartInventoryManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    
    public class StockPredictionController : ControllerBase
    {
        private readonly IStockPredictionService _predictionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly AppDbContext _context;

        public StockPredictionController(
            IStockPredictionService predictionService,
            AppDbContext context,
            ICurrentUserService currentUserService)
        {
            _predictionService = predictionService;
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet("{productId}/prediction")]
        public async Task<IActionResult> GetPrediction(int productId)
        {
            var userId = _currentUserService.UserId;

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == productId &&
                    p.UserId == userId);

            if (product == null)
                return NotFound();

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var saleItemsLast30Days = await _context.SaleItems
                .Include(si => si.Sale)
                .Where(si =>
                    si.ProductId == productId &&
                    si.Sale != null &&
                    si.Sale.UserId == userId &&
                    si.Sale.SaleDate >= thirtyDaysAgo)
                .ToListAsync();

            var prediction = _predictionService
                .Predict(product, saleItemsLast30Days);

            return Ok(prediction);
        }

    }
}
