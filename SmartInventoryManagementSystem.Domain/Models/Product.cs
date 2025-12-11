using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Domain.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime ExpiryDate { get; set; }

        public decimal ProductPrice { get; set; }

        // FK to Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // One-to-many: one product can appear in many sales
        public ICollection<Sale>? Sales { get; set; }
    }
}
