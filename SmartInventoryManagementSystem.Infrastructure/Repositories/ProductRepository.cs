using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.DTO.ProductDTO;
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
            
            return await _context.Products
                
                .Where(p => p.UserId == _currentUser.UserId && p.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task <string?> AddProductAsync(Product? product)
        {
            if (product == null)
                return "Product is null";

            if (product.ExpiryDate <= DateTime.Today)
                return "Expiry date must be in the future";

            product.UserId = _currentUser.UserId;
            await _context.Products.AddAsync(product);
           await _context.SaveChangesAsync();
            return null;    
        }

        public async Task <Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
        .AsNoTracking()
        .FirstOrDefaultAsync(p =>
            p.ProductId == id &&
            p.UserId == _currentUser.UserId &&
            p.IsActive
            );
        }

        public async Task<string?> UpdateProductAsync(Product? updated)
        {
            if (updated == null)
                return "Product is null";

            
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == updated.ProductId);

            if (product == null)
                return "Product not found";

            
            if (updated.ExpiryDate <= DateTime.Today)
                return "Expiry date must be in the future";

            
            product.ProductName = updated.ProductName;
            product.QuantityInStock = updated.QuantityInStock;
            product.ReorderLevel = updated.ReorderLevel;
            product.ProductPrice = updated.ProductPrice;
            product.ExpiryDate = updated.ExpiryDate;

            await _context.SaveChangesAsync();
            return null;
        }


        public async Task<string?> DeleteProductAsync(int id)
        {
            var product = await _context.Products
        .Include(p => p.SaleItems)
        .FirstOrDefaultAsync(p =>
            p.ProductId == id &&
            p.UserId == _currentUser.UserId);

            if (product == null)
                return "Product not found";


            if (product.SaleItems?.Any() == true)
            {
                
                product.IsActive = false;
            }

            else
            {

                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return null;

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
