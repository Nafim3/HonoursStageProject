using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO.StockPredictionDTO
{
    public class StockPredictionResult
    {
        public double AverageDailySales { get; set; }

        public double DaysRemaining { get; set; }

        public bool ShouldReorder { get; set; }

        public int SuggestedReorderQuantity { get; set; }

        public string? RiskLevel { get; set; }
    }
}
