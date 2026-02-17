using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO.SaleDTO
{
    public class CreateSaleResponse
    {
        public int SaleId { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; } 
    }
}
