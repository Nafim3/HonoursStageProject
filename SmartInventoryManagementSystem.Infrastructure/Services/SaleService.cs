using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly bool _disableTransactions;

        // Production constructor (used by ASP.NET Core DI)
        public SaleService(AppDbContext context, ICurrentUserService currentUser, INotificationService notificationService)
        {
            _context = context;
            _currentUserService = currentUser;
            _notificationService = notificationService;
            _disableTransactions = false;
        }

        // Test constructor (used only in unit tests)
        public SaleService(AppDbContext context, ICurrentUserService currentUser, INotificationService notificationService, bool disableTransactions)
        {
            _context = context;
            _currentUserService = currentUser;
            _notificationService = notificationService;
            _disableTransactions = disableTransactions;
        }

        public async Task<CreateSaleResponse> CreateSaleAsync(CreateSaleRequest request)
        {
           
            if (request.Items == null || !request.Items.Any())
                throw new ArgumentException("Sale must contain at least one item");

            var duplicateProduct = request.Items
                .GroupBy(i => i.ProductId)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateProduct != null)
                throw new ArgumentException($"Product {duplicateProduct.Key} appears multiple times in the sale");

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero");
            }

            

            var currentUserId = _currentUserService.UserId;

            var productIds = request.Items.Select(i => i.ProductId).ToList();

            var products = await _context.Products
                .Where(p => p.UserId == currentUserId && productIds.Contains(p.ProductId))
                .ToListAsync();

            foreach (var item in request.Items)
            {
                var product = products.FirstOrDefault(p => p.ProductId == item.ProductId);

                if (product == null)
                    throw new Exception($"Product with ID {item.ProductId} not found");

                if (product.ExpiryDate <= DateTime.UtcNow)
                    throw new Exception($"Product {product.ProductName} is expired and cannot be sold");



                if (product.QuantityInStock < item.Quantity)
                    throw new Exception($"Not enough stock for product {product.ProductName}");
            }

            

            IDbContextTransaction? transaction = null;

            if (!_disableTransactions)
            {
                transaction = await _context.Database.BeginTransactionAsync();
            }

            try
            {
               

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
                    var product = products.First(p => p.ProductId == item.ProductId);

                    var lineTotal = product.ProductPrice * item.Quantity;

                    var saleItem = new SaleItem
                    {
                        SaleId = sale.SaleId,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.ProductPrice,
                        LineTotal = lineTotal
                    };

                    saleItems.Add(saleItem);

                  
                    product.QuantityInStock -= item.Quantity;

                   
                    if (product.QuantityInStock <= product.ReorderLevel)
                    {
                        await _notificationService.NotifyLowStockAsync(product, currentUserId);
                    }
                }

               

                await _context.SaleItems.AddRangeAsync(saleItems);

                sale.TotalAmount = saleItems.Sum(si => si.LineTotal);

                await _context.SaveChangesAsync();

              

                if (transaction != null)
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
                if (transaction != null)
                    await transaction.RollbackAsync();

                throw;
            }




        }

    }
}
