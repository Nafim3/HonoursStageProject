using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO
{
    public class CreateSaleResponse
    {
        public int SaleId { get; set; }
        public decimal TotalAmount { get; set; }

        // New property to indicate number of items in the sale
        public int ItemCount { get; set; } 
    }
}
