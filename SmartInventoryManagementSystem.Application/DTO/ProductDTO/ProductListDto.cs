using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// even if the product.cs and this dto's property are the same, it's better to keep them separate for future changes

namespace SmartInventoryManagementSystem.Application.DTO.ProductDTO
{
    public class ProductListDto
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal Price { get; set; }

    }
}
