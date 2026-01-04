using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly INotificationService _notificationService;

        public ProductService(
            IProductRepository productRepository,
            INotificationService notificationService)
        {
            _productRepository = productRepository;
            _notificationService = notificationService;
        }

        public IEnumerable<Product> GetLowStockProducts()
        {
            return _productRepository.GetLowStockProducts();
        }

        public void CheckLowStockAndNotify()
        {
            var lowStockProducts = _productRepository.GetLowStockProducts();

            foreach (var product in lowStockProducts)
            {
                _notificationService.NotifyLowStock(product);
            }
        }
    }
}
