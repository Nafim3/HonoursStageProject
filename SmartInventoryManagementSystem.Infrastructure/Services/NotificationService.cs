using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Application.Interfaces;
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

        public async Task NotifyLowStockAsync(Product product)
        {
           

            var alert = new Alert
            {
                Category = "LowStock",
                Message = $"{product.ProductName} is low in stock (Qty: {product.QuantityInStock})",
                CreatedAt = DateTime.UtcNow
            };

            await _alertRepository.AddAsync(alert);
        }

        public async Task NotifyExpiredProductsAsync(Product product)
        {
          

            var alert = new Alert
            {
                Category = "Expired",
                Message = $"{product.ProductName} expired on {product.ExpiryDate:d}",
                CreatedAt = DateTime.UtcNow
            };

            await _alertRepository.AddAsync(alert);
        }
    }
}

/*
 public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task NotifyLowStockAsync(Product product)
    {
        _context.Notifications.Add(new Notification
        {
            Category = "LowStock",
            Message = $"LOW STOCK: {product.ProductName} (Qty: {product.QuantityInStock})"
        });

        await _context.SaveChangesAsync();
    }

    public async Task NotifyExpiredProductsAsync(Product product)
    {
        _context.Notifications.Add(new Notification
        {
            Category = "Expired",
            Message = $"EXPIRED: {product.ProductName} (Expired on {product.ExpiryDate:d})"
        });

        await _context.SaveChangesAsync();
    }
}
 */ 