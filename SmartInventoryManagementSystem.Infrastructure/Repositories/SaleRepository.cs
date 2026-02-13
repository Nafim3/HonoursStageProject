using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.DTO.SaleDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public SaleRepository(AppDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task <List <GetAllSales>> FetchAllSalesAsync()
        {
            int userId = _currentUserService.UserId;
            return await _context.Sales
                        .Where (sale => sale.UserId == userId)
                        .Select (sale=> new GetAllSales
                        {
                            SaleId = sale.SaleId,
                            
                            SaleDate = sale.SaleDate,
                            TotalAmount = sale.TotalAmount
                        })
                        .AsNoTracking()
                        .ToListAsync();
        }

        public async Task <GetByID?> FetchSaleByIdAsync(int saleId)
        {
            int userId = _currentUserService.UserId;
            return await _context.Sales
                        .Where(sale => sale.SaleId == saleId && sale.UserId == userId)
                        .Select(sale => new GetByID
                        {
                            SaleId = sale.SaleId,
                            SaleDate = sale.SaleDate,
                            TotalAmount = sale.TotalAmount,
                            BuyerName = sale.BuyerName,
                            Items = sale.SaleItems
                                        .Select(item => new SaleDetails
                                        {
                                            ProductId = item.ProductId,
                                            ProductName = item.Product!.ProductName,
                                            QuantitySold = item.Quantity,
                                            ProductPrice = item.UnitPrice,
                                            LineTotal =  item.LineTotal
                                        })
                                        .ToList()
                        })
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
        }
    }
}
