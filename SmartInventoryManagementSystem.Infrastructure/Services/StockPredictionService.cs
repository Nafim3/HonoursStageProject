using SmartInventoryManagementSystem.Application.DTO.StockPredictionDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.Services
{
    public class StockPredictionService : IStockPredictionService
    {
        public StockPredictionResult Predict(Product product,List<SaleItem> saleItemsLast30Days)
        {
            var totalSold = saleItemsLast30Days.Sum(si => si.Quantity);

            double averageDailySales = totalSold / 30.0;

            if (averageDailySales <= 0)
                averageDailySales = 1;

            double daysRemaining = product.QuantityInStock / averageDailySales;

            bool shouldReorder = daysRemaining < 7;

            int suggestedReorderQuantity = 0;

            if (shouldReorder)
            {
                suggestedReorderQuantity =
                    (int)((averageDailySales * 14) - product.QuantityInStock);

                if (suggestedReorderQuantity < 0)
                    suggestedReorderQuantity = 0;

            }


            string riskLevel;

            if (daysRemaining < 3)
                riskLevel = "Critical";
            else if (daysRemaining < 7)
                riskLevel = "High";
            else if (daysRemaining < 14)
                riskLevel = "Medium";
            else
                riskLevel = "Safe";

            return new StockPredictionResult
            {
                AverageDailySales = averageDailySales,
                DaysRemaining = daysRemaining,
                ShouldReorder = shouldReorder,
                SuggestedReorderQuantity = suggestedReorderQuantity,
                RiskLevel = riskLevel
            };
        }
    }
}
