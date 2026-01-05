using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotifyLowStockAsync(Product product);
        Task NotifyExpiredProductsAsync(Product product); 

    }
}
