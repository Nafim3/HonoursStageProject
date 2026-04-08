using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SmartInventoryManagementSystem.ApiTests.Helper;
using SmartInventoryManagementSystem.Application.DTO.StockPredictionDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SmartInventoryManagementSystem.IntegrationTests.PredictionEndpoints
{
    public class PredictionEndpointTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public PredictionEndpointTests(ApiFactory factory)
        {
            _factory = factory;
        }
        
        [Fact]
        public async Task GetPrediction_ReturnsMockedPrediction()
        {
            var mockPrediction = new Mock<IStockPredictionService>();
            mockPrediction
                .Setup(x => x.Predict(It.IsAny<Product>(), It.IsAny<List<SaleItem>>()))
                .Returns(new StockPredictionResult
                {
                    AverageDailySales = 1,
                    DaysRemaining = 10,
                    ShouldReorder = false,
                    SuggestedReorderQuantity = 0,
                    RiskLevel = "Safe"
                });


            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
                });
            });


            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Products.Add(new Product
            {
                ProductId = 1,
                UserId = 1,
                ProductName = "Test Product",
                QuantityInStock = 10
            });

            db.SaveChanges();


            var client = factory.CreateClient();

            var response = await client.GetAsync("api/StockPrediction/prediction");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Fact]
        public async Task GetPrediction_ReturnsEmptyList_WhenUserHasNoProducts()
        {

            var mockPrediction = new Mock<IStockPredictionService>();

            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {

                    services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

                    services.RemoveAll(typeof(IStockPredictionService));
                    services.AddSingleton(mockPrediction.Object);
                });
            });

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test");


            var response = await client.GetAsync("/api/StockPrediction/prediction");
            var json = await response.Content.ReadAsStringAsync();


            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = JsonSerializer.Deserialize<List<PredictionWrapper>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result.Should().BeEmpty();

            mockPrediction.Verify(
                x => x.Predict(It.IsAny<Product>(), It.IsAny<List<SaleItem>>()),
                Times.Never);

        }



        [Fact]
        public async Task GetPrediction_CallsPredictionOncePerProduct()
        {

            var mockPrediction = new Mock<IStockPredictionService>();
            mockPrediction
                .Setup(x => x.Predict(It.IsAny<Product>(), It.IsAny<List<SaleItem>>()))
                .Returns(new StockPredictionResult());

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IStockPredictionService));
                    services.AddSingleton(mockPrediction.Object);
                });
            }).CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Products.Add(new Product { ProductId = 1, UserId = 1, ProductName = "P1" });
            db.Products.Add(new Product { ProductId = 2, UserId = 1, ProductName = "P2" });

            db.SaveChanges();


            await client.GetAsync("/api/StockPrediction/prediction");


            mockPrediction.Verify(
                x => x.Predict(It.IsAny<Product>(), It.IsAny<List<SaleItem>>()),
                Times.Exactly(2));
        }


    }
}
