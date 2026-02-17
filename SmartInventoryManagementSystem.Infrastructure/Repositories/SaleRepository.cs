using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.DTO.SaleDTO;
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
    public class SaleRepository : ISaleRepository
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public SaleRepository(AppDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task <List <Sale>> FetchAllSalesAsync()
        {
            int userId = _currentUserService.UserId;
            return await _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(i => i.Product)
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        public async Task <Sale?> FetchSaleByIdAsync(int saleId)
        {
            int userId = _currentUserService.UserId;
            return await _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(i => i.Product)
                .Where(s => s.SaleId == saleId && s.UserId == userId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
