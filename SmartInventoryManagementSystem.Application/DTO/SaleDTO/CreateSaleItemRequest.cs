using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO.SaleDTO
{
    public class CreateSaleItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
