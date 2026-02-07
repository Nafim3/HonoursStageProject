using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {

        private readonly IAlertRepository _alertRepository;

        public NotificationService(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        public async Task NotifyLowStockAsync(Product product, int userId)
        {
            var exists = await _alertRepository.ExistAsync
            (
                userId,
                "Low Stock",
                product.ProductId
            );
           
            if (exists)
                return;

            var alert = new Alert
            {
                Category = "Low Stock",
                Message = $"{product.ProductName} is low in stock (Quantity: {product.QuantityInStock})",
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ProductId = product.ProductId
            };

            await _alertRepository.AddAsync(alert);
        }

        public async Task NotifyExpiredProductsAsync(Product product, int userId)
        {
            var exists = await _alertRepository.ExistAsync
            (
                userId,
                "Expired",
                product.ProductId
            );

            if (exists)
                return;


            var alert = new Alert
            {
                Category = "Expired",
                Message = $"{product.ProductName} expired on {product.ExpiryDate:d}",
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ProductId = product.ProductId
            };

            await _alertRepository.AddAsync(alert);
        }

        public async Task<List<AlertDTO>> GetBellAlertsAsync(int userId)
        {
            var expired = await _alertRepository.GetRecentAsync(userId, "Expired", 5);
            var lowStock = await _alertRepository.GetRecentAsync(userId, "Low Stock", 5);

            return expired
                .Concat(lowStock)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AlertDTO
                {
                    Category = a.Category,
                    Message = a.Message,
                    CreatedAt = a.CreatedAt
                })
                .ToList();

        }
    }
}
