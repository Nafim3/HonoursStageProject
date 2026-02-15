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
        public decimal TotalAmount { get; set; }
        public DateTime SaleDate { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        // FKs
        public int UserId { get; set; }
        // Navigation properties
        public User? User { get; set; }
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
