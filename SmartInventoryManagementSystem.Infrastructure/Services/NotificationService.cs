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
        public void NotifyLowStock(Product product)
        {
            Console.WriteLine($"LOW STOCK ALERT: {product.ProductName}, Quantity: {product.QuantityInStock}");
        }
    }
}
