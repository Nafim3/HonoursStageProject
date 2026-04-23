using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.UnitTests.InfrastructureTests.ServicesTests
{
    public class PredictionServiceTests
    {
        private readonly StockPredictionService _service;

        public PredictionServiceTests()
        {
            _service = new StockPredictionService();
        }

        [Fact]
        public void Predict_WhenNoSales_ShouldUseAverageDailySalesOf1()
        {

            var product = new Product { QuantityInStock = 30 };
            var saleItems = new List<SaleItem>(); // no sales

            var result = _service.Predict(product, saleItems);

            Assert.Equal(0, result.AverageDailySales);
            Assert.Equal(0, result.DaysRemaining);
            Assert.False(result.ShouldReorder);
            Assert.Equal("Safe", result.RiskLevel);
        }

        [Fact]
        public void Predict_WhenDaysRemainingLessThan3_ShouldBeCritical()
        {
            var product = new Product { QuantityInStock = 2 };
            var saleItems = new List<SaleItem>
        {
            new SaleItem { Quantity = 30 } // 1 per day
        };

            var result = _service.Predict(product, saleItems);

            Assert.Equal("Critical", result.RiskLevel);
            Assert.True(result.ShouldReorder);
        }

        [Fact]
        public void Predict_WhenDaysRemainingLessThan7_ShouldBeHigh()
        {
            var product = new Product { QuantityInStock = 5 };
            var saleItems = new List<SaleItem>
        {
            new SaleItem { Quantity = 30 } 
        };

            var result = _service.Predict(product, saleItems);

            Assert.Equal("High", result.RiskLevel);
        }

        [Fact]
        public void Predict_WhenReorderNeeded_ShouldCalculateSuggestedQuantity()
        {
            var product = new Product { QuantityInStock = 5 };
            var saleItems = new List<SaleItem>
        {
                // 2 per day
            new SaleItem { Quantity = 60 } 
        };

            var result = _service.Predict(product, saleItems);

            // 2 per day * 14 days = 28 needed
            // 28 - 5 in stock = 23
            Assert.Equal(23, result.SuggestedReorderQuantity);
        }

        [Fact]
        public void Predict_WhenSuggestedQuantityNegative_ShouldReturnZero()
        {
            var product = new Product { QuantityInStock = 100 };
            var saleItems = new List<SaleItem>
        {
               
            new SaleItem { Quantity = 30 } 
        };

            var result = _service.Predict(product, saleItems);

            Assert.Equal(0, result.SuggestedReorderQuantity);
        }


    }
}
