using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Domain.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public int QuantitySold { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SaleDate { get; set; }

        // FKs
        public int ProductId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Product? Product { get; set; }
        public User? User { get; set; }
    }
}
