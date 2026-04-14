using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.DTO.StockPredictionDTO;
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

        [HttpGet("prediction")]
        public async Task<IActionResult> GetPrediction()
        {
            var userId = _currentUserService.UserId;

            var products = await _context.Products
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var allSaleItems = await _context.SaleItems
                .Include(si => si.Sale)
                .Where(si =>
                    si.Sale != null &&
                    si.Sale.UserId == userId &&
                    si.Sale.SaleDate >= thirtyDaysAgo)
                .ToListAsync();

            var results = products.Select(product =>
            {
                var saleItems = allSaleItems
                    .Where(si => si.ProductId == product.ProductId)
                    .ToList();

                var prediction = _predictionService.Predict(product, saleItems);

                return new PredictionWrapper
                {
                    PName = product.ProductName!,
                    Prediction = prediction
                };
            }).ToList();

            return Ok(results);
        }


    }
}
