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
    public class AlertRepository : IAlertRepository
    {
        private readonly AppDbContext _context;

        public AlertRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Alert alert)
        {
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Alert>> GetRecentAsync(int userId, string category, int limit)
        {
            return await _context.Alerts
                .Where(a => a.UserId == userId && a.Category == category)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistAsync(int userId, string category, int? productId)
        {
            return await _context.Alerts
                .AnyAsync(a => 
                a.UserId == userId && 
                a.Category == category && 
                a.ProductId == productId
                );
        }

        public async Task<int> CountAsync(int userId, string category)
        {
            return await _context.Alerts
            .Where(a => a.UserId == userId && a.Category == category)
            .CountAsync();
        }

        public async Task<Alert?> GetByIdAsync(int id)
        {
            return await _context.Alerts.FirstOrDefaultAsync(a => a.Id == id);
        }


        public async Task DeleteAsync(Alert alert)
        {
            if (alert != null)
            {
                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Alert>> GetAllForUserAsync(int userId)
        {
            return await _context.Alerts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }


    }
}