using SmartInventoryManagementSystem.Application.DTO.SaleDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface IPdfService
    {
       Task <byte[]> GenerateInvoicePdf(GetByID sale);
    }
}
