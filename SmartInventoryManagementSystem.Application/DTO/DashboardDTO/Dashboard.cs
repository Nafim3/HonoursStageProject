using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO.DashboardDTO
{
    public class Dashboard
    {
        public int TotalProducts { get; set; }
        public int SalesToday { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiredProductsCount { get; set; }   
    }
}
