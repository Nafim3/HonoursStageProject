using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface IProductRepository
    {
        

        Task <List<Product>> GetProductAsync();
        Task <Product?> GetProductByIdAsync(int id);
        Task <string?> AddProductAsync(Product? product);
        Task <string?> UpdateProductAsync(Product? product);
        Task <string?> DeleteProductAsync(int id);
        Task <List<Product>> GetExpiredProductsAsync(int userId);
        Task<List<Product>> GetLowStockProductsAsync(int userId);


    }
}
