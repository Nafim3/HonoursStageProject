using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.PDF;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.UnitTests.InfrastructureTests.ServicesTests
{
    public class PdfTest
    {
        [Fact]
        public void BuildFromEntity_IncludesInvoiceMetadata()
        {
            var sale = TestData.CreateSampleSale();

            var html = BuildInvoice.BuildFromEntity(sale);

            html.Should().Contain("Invoice #:");
            html.Should().Contain("100");
            html.Should().Contain("2024-01-15");
            html.Should().Contain("Mahin");
        }

        [Fact]
        public void BuildFromEntity_RendersLineItems()
        {
            var sale = TestData.CreateSampleSale();

            var html = BuildInvoice.BuildFromEntity(sale);

            html.Should().Contain("Keyboard");
            html.Should().Contain("2");
            html.Should().Contain("£50.00");   // currency formatting
            html.Should().Contain("£100.00");

            html.Should().Contain("Mouse");
            html.Should().Contain("1");
            html.Should().Contain("£50.00");
            html.Should().Contain("£50.00");
        }

        [Fact]
        public void BuildFromEntity_IncludesGrandTotal()
        {
            var sale = TestData.CreateSampleSale();

            var html = BuildInvoice.BuildFromEntity(sale);

            html.Should().Contain("Grand Total");
            html.Should().Contain("£150.00");
        }

        [Fact]
        public void BuildFromEntity_ProducesValidHtmlStructure()
        {
            var sale = TestData.CreateSampleSale();

            var html = BuildInvoice.BuildFromEntity(sale);

            html.Should().Contain("<html>");
            html.Should().Contain("</html>");
            html.Should().Contain("<table>");
            html.Should().Contain("</table>");
            html.Should().Contain("<tbody>");
            html.Should().Contain("</tbody>");
        }


        private static class TestData
        {
            public static Sale CreateSampleSale()
            {
                return new Sale
                {
                    SaleId = 100,
                    SaleDate = new DateTime(2024, 1, 15),
                    BuyerName = "Mahin",
                    TotalAmount = 150.00m,
                    SaleItems = new List<SaleItem>
            {
                new SaleItem
                {
                    Product = new Product { ProductName = "Keyboard" },
                    Quantity = 2,
                    UnitPrice = 50.00m,
                    LineTotal = 100.00m
                },
                new SaleItem
                {
                    Product = new Product { ProductName = "Mouse" },
                    Quantity = 1,
                    UnitPrice = 50.00m,
                    LineTotal = 50.00m
                }
            }
                };
            }
        }



    }
}
