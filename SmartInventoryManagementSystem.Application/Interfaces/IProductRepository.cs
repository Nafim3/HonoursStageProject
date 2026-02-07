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
        // Methods that describe what operations are allowed on Products

        Task <List<Product>> GetProductAsync();
        Task <Product?> GetProductByIdAsync(int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task <List<Product>> GetExpiredProductsAsync(int userId);


    }
}
