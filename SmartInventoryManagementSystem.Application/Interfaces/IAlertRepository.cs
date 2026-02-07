using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface IAlertRepository
    {
        Task AddAsync(Alert alert);
        Task<List<Alert>> GetRecentAsync(int userId, string category, int limit);
        Task <bool> ExistAsync(int userId, string category, int? productId);
        Task<int> CountAsync(int userId, string category);

    }
}
