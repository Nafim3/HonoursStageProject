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

        public CreateSaleResponse CreateSale(CreateSaleRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // check for empty sale
                if (request.Items == null || !request.Items.Any())
                {
                    throw new ArgumentException("Sale must contain at least one item.");
                }

                // Duplicate product check
                var duplicateProduct = request.Items
                    .GroupBy(i => i.ProductId)
                    .FirstOrDefault(g => g.Count() > 1);

                if (duplicateProduct != null)
                {
                    throw new ArgumentException(
                        $"Product {duplicateProduct.Key} appears multiple times in the sale.");
                }

                // User existence check
                var userExists = _context.Users.Any(u => u.UserId == request.UserId);
                if (!userExists)
                    throw new Exception("Invalid user.");

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
                    // First validate quantity
                    if (item.Quantity <= 0)
                        throw new ArgumentException("Quantity must be greater than zero.");

                    // THIS is where the product is fetched
                    var product = _context.Products
                        .FirstOrDefault(p => p.ProductId == item.ProductId);

                    // Then its validated
                    if (product == null)
                        throw new Exception($"Product with ID {item.ProductId} not found.");

                    // Then stock is checked
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
                transaction.Commit();

                return new CreateSaleResponse
                {
                    SaleId = sale.SaleId,
                    TotalAmount = sale.TotalAmount,
                    ItemCount = saleItems.Count
                };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
