using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Domain.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }

        // One-to-many: one category can contain many products
        public ICollection<Product>? Products { get; set; }
    }
}
