using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO.StockPredictionDTO
{
    public class PredictionWrapper
    {
        public string PName { get; set; } = "";
        public StockPredictionResult Prediction { get; set; } = new();
    }
}
