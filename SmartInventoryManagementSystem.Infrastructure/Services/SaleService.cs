using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.DTO.SaleDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.Services
{
    public class SaleService : ISaleService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService; // Placeholder for current user ID
        private readonly INotificationService _notificationService; // For low stock notifications
        public SaleService(AppDbContext context, ICurrentUserService currentUser, INotificationService notificationService)
        {
            _context = context;
            _currentUserService = currentUser;
            _notificationService = notificationService;
        }

        public async Task<CreateSaleResponse> CreateSaleAsync(CreateSaleRequest request)
        {
            var currentUserId = _currentUserService.UserId;
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Empty sale check
                if (request.Items == null || !request.Items.Any())
                    throw new ArgumentException("Sale must contain at least one item.");

                // Duplicate product check
                var duplicateProduct = request.Items
                    .GroupBy(i => i.ProductId)
                    .FirstOrDefault(g => g.Count() > 1);

                if (duplicateProduct != null)
                    throw new ArgumentException($"Product {duplicateProduct.Key} appears multiple times in the sale.");

                // Create sale
                var sale = new Sale
                {
                    UserId = currentUserId,
                    SaleDate = DateTime.UtcNow,
                    BuyerName = request.BuyerName,
                    TotalAmount = 0
                };

                await _context.Sales.AddAsync(sale);
                await _context.SaveChangesAsync();

                var saleItems = new List<SaleItem>();

                foreach (var item in request.Items)
                {
                    if (item.Quantity <= 0)
                        throw new ArgumentException("Quantity must be greater than zero.");

                    // Fetch product async
                    var product = await _context.Products
                              .FirstOrDefaultAsync(p =>
                              p.ProductId == item.ProductId &&
                              p.UserId == currentUserId);


                    if (product == null)
                        throw new Exception($"Product with ID {item.ProductId} not found.");

                    if (product.QuantityInStock < item.Quantity)
                        throw new Exception($"Not enough stock for product {product.ProductName}.");

                    var lineTotal = product.ProductPrice * item.Quantity;

                    // Create sale item
                    var saleItem = new SaleItem
                    {
                        SaleId = sale.SaleId,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.ProductPrice,
                        LineTotal = lineTotal
                    };

                    saleItems.Add(saleItem);

                    // Update stock
                    product.QuantityInStock -= item.Quantity;

                    if (product.QuantityInStock <= product.ReorderLevel)
                    {
                        await _notificationService.NotifyLowStockAsync(product, currentUserId);
                    }

                }

                await _context.SaleItems.AddRangeAsync(saleItems);

                sale.TotalAmount = saleItems.Sum(si => si.LineTotal);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateSaleResponse
                {
                    SaleId = sale.SaleId,
                    TotalAmount = sale.TotalAmount,
                    ItemCount = saleItems.Count
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
