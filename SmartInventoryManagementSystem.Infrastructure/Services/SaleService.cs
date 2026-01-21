using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartInventoryManagementSystem.Domain.Models;

namespace SmartInventoryManagementSystem.Infrastructure.Services
{
    public class SaleService : ISaleService
    {
        private readonly AppDbContext _context;

        public SaleService(AppDbContext context)
        {
            _context = context;
        }

        public int CreateSale(CreateSaleRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                throw new ArgumentException("Sale must contain at least one item.");
            }
            var sale = new Sale
            {
                UserId = request.UserId,
                SaleDate = DateTime.UtcNow,
                TotalAmount = 0
            }; 
            _context.Sales.Add(sale);
            _context.SaveChanges();

            var saleItems = new List<SaleItem>();

            foreach (var item in request.Items)
            {
                // THIS is where you fetch the product
                var product = _context.Products
                    .FirstOrDefault(p => p.ProductId == item.ProductId);

                // Then you validate it
                if (product == null)
                    throw new Exception($"Product with ID {item.ProductId} not found.");

                // Then you check stock
                if (product.QuantityInStock < item.Quantity)
                    throw new Exception($"Not enough stock for product {product.ProductName}.");

                // Then calculate totals
                var lineTotal = product.ProductPrice * item.Quantity;

                // Then create the SaleItem
                var saleItem = new SaleItem
                {
                    SaleId = sale.SaleId,
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.ProductPrice,
                    LineTotal = lineTotal
                };

                saleItems.Add(saleItem);
                

                // Then update stock
                product.QuantityInStock -= item.Quantity;
            }
            _context.SaleItems.AddRange(saleItems);

            sale.TotalAmount = saleItems.Sum(si => si.LineTotal);

            _context.SaveChanges();

            return sale.SaleId;
        }
    }
}
