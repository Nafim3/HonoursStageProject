using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.DTO.Analytics;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AnalyticsService(AppDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }
        public async Task <List<SalesAnalysis>> GetSalesAnalysisAsync()
        {
            var userId = _currentUserService.UserId;
            return await _context.Sales
                .Where(s => s.UserId == userId)
                .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                .Select(g => new SalesAnalysis
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(s => s.TotalAmount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();
        }
    }
}
