using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO
{
    public class SaleDetails
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }="";
        public int QuantitySold { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
