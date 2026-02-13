using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO.SaleDTO
{
    public class CreateSaleRequest
    {
        public string BuyerName { get; set; } = string.Empty;
        public List <CreateSaleItemRequest> Items { get; set; } = new();
    }
}
