using SmartInventoryManagementSystem.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface ISaleService
    {
        CreateSaleResponse CreateSale(CreateSaleRequest request);
    }
}
