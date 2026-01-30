using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository (AppDbContext context)
        {
            _context = context;
        }

        public async Task <List<Product>> GetProductAsync()
        {

            if (_context.Products == null)
            {
                throw new InvalidOperationException("There's no product in the table");
            }
            // Eager loading Category and Sales related data
            return await _context.Products
                //.Include(p => p.Sales)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddProductAsync(Product product)
        {
           await _context.Products.AddAsync(product);
           await _context.SaveChangesAsync();
        }

        public async Task <Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                //.Include(p => p.Sales)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task UpdateProductAsync(Product product)
        {
           _context.Products.Update(product);
           await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
               await _context.SaveChangesAsync();
            }
        }
        public async Task <List<Product>> GetLowStockProductsAsync()
        {
            return await _context.Products
                .Where(p => p.QuantityInStock <= p.ReorderLevel)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task <List<Product>> GetExpiredProductsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Products
                .Where(p => p.ExpiryDate <= currentDate)
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
