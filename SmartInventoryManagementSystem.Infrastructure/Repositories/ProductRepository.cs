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
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sales)
                .AsNoTracking()
                .ToList();
        }


    }
}
