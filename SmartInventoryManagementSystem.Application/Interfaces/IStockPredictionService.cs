using SmartInventoryManagementSystem.Application.DTO.StockPredictionDTO;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface IStockPredictionService
    {
       StockPredictionResult Predict(Product product, List<SaleItem> salesLast30Days);
    }
}
