using SmartInventoryManagementSystem.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface ISaleRepository
    {
        IEnumerable<GetAllSales> FetchAllSales();
        GetByID? FetchSaleById(int saleId);
    }
}
