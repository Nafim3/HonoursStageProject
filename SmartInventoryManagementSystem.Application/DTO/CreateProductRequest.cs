using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO
{
    public class CreateProductRequest
    {
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal Price { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
