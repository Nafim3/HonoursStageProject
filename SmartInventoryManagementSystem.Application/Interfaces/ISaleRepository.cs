using SmartInventoryManagementSystem.Application.DTO.SaleDTO;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface ISaleRepository
    {
       Task<List<Sale>> FetchAllSalesAsync();
       Task<Sale?> FetchSaleByIdAsync(int saleId);
    }
}
