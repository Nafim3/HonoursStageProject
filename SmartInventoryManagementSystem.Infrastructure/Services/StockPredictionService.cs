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

            // Handle no sales properly
            if (totalSold <= 0)
            {
                return new StockPredictionResult
                {
                    AverageDailySales = 0,
                    DaysRemaining = 0,
                    ShouldReorder = false,
                    SuggestedReorderQuantity = 0,
                    RiskLevel = "Safe"
                };
            }

            double averageDailySales = totalSold / 30.0;

            double daysRemaining = product.QuantityInStock / averageDailySales;

            bool shouldReorder = daysRemaining < 7;

            int suggestedReorderQuantity = 0;

            if (shouldReorder)
            {
                suggestedReorderQuantity =
                    (int)((averageDailySales * 14) - product.QuantityInStock);

                suggestedReorderQuantity = Math.Max(0, suggestedReorderQuantity);
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
