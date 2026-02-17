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
        public DateTime? ExpiryDate { get; set; }
        public decimal ProductPrice { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; } = true; 
        public ICollection<SaleItem>? SaleItems { get; set; }
    }
}
