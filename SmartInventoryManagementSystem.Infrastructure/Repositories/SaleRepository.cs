using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.DTO;
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

        public SaleRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable <GetAllSales> FetchAllSales()
        {
            return _context.Sales
                        .Select (sale=> new GetAllSales
                        {
                            SaleId = sale.SaleId,
                            UserId = sale.UserId,
                            SaleDate = sale.SaleDate,
                            TotalAmount = sale.TotalAmount
                        })
                        .AsNoTracking()
                        .ToList();
        }

        public GetByID? FetchSaleById(int saleId)
        {
            return _context.Sales
                        .Where(sale => sale.SaleId == saleId)
                        .Select(sale => new GetByID
                        {
                            SaleId = sale.SaleId,
                            SaleDate = sale.SaleDate,
                            TotalAmount = sale.TotalAmount,
                            Items = sale.SaleItems
                                        .Select(item => new SaleDetails
                                        {
                                            ProductId = item.ProductId,
                                            ProductName = item.Product!.ProductName,
                                            QuantitySold = item.Quantity,
                                            ProductPrice = item.UnitPrice,
                                            LineTotal =  item.UnitPrice
                                        })
                                        .ToList()
                        })
                        .AsNoTracking()
                        .FirstOrDefault();
        }
    }
}
