using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface IProductService
    {
        Task <IEnumerable<Product>> GetLowStockProductsFromDBAsync();
        Task CheckLowStockAndNotifyAsync();
        Task<IEnumerable<Product>> GetExpiredProductsFromDBAsync();
        Task CheckExpiredProductsAndNotifyAsync();
    }
}
