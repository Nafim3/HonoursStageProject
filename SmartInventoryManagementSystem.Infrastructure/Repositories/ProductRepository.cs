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

        public IEnumerable<Product> GetProduct()
        {

            if (_context.Products == null)
            {
                throw new InvalidOperationException("There's no product in the table");
            }
            // Eager loading Category and Sales related data
            return _context.Products
                .Include(p => p.Sales)
                .AsNoTracking()
                .ToList();
        }

        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public Product? GetProductById(int id)
        {
            return _context.Products
                .Include(p => p.Sales)
                .AsNoTracking()
                .FirstOrDefault(p => p.ProductId == id);
        }

        public void UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }

        public void DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }

    }
}
