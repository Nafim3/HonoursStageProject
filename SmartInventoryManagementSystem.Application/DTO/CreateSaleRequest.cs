using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO
{
    public class CreateSaleRequest
    {
        public int UserId { get; set; }
        public List <CreateSaleItemRequest> Items { get; set; } = new();
    }
}
