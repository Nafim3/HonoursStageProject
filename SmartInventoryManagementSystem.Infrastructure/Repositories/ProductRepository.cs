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
        private readonly ICurrentUserService _currentUser;
        public ProductRepository (AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task <List<Product>> GetProductAsync()
        {

            if (_context.Products == null)
            {
                throw new InvalidOperationException("There's no product in the table");
            }
            // Eager loading Category and Sales related data
            return await _context.Products
                
                .Where(p => p.UserId == _currentUser.UserId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            product.UserId = _currentUser.UserId;
            await _context.Products.AddAsync(product);
           await _context.SaveChangesAsync();
        }

        public async Task <Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
        .AsNoTracking()
        .FirstOrDefaultAsync(p =>
            p.ProductId == id &&
            p.UserId == _currentUser.UserId);
        }

        public async Task UpdateProductAsync(Product product)
        {
           _context.Products.Update(product);
           await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products
         .FirstOrDefaultAsync(p =>
             p.ProductId == id &&
             p.UserId == _currentUser.UserId);

            if (product == null)
                return;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        public async Task <List<Product>> GetLowStockProductsAsync(int userId)
        {
            return await _context.Products
                 .Where(p =>
            p.UserId == _currentUser.UserId &&
            p.QuantityInStock <= p.ReorderLevel)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task <List<Product>> GetExpiredProductsAsync(int userId)
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Products
                 .Where(p =>
            p.UserId == _currentUser.UserId &&
            p.ExpiryDate <= currentDate)
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
