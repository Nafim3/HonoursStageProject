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

        public async Task <IEnumerable<Product>> GetLowStockProductsFromDBAsync()
        {
            return await _productRepository.GetLowStockProductsAsync();
        }

        public async Task CheckLowStockAndNotifyAsync()
        {
            var lowStockProducts = await _productRepository.GetLowStockProductsAsync();

            foreach (var product in lowStockProducts)
            {
               await _notificationService.NotifyLowStockAsync(product);
            }
        }

        public async Task<IEnumerable<Product>> GetExpiredProductsFromDBAsync()
        {
            return await _productRepository.GetExpiredProductsAsync();
        }


        public async Task CheckExpiredProductsAndNotifyAsync()
        {
            var expiredProducts = await _productRepository.GetExpiredProductsAsync();
            foreach (var product in expiredProducts)
            {
               await _notificationService.NotifyExpiredProductsAsync(product);
            }
        }
    }
}
