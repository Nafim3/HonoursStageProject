using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Infrastructure.Repositories;
using SmartInventoryManagementSystem.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.UnitTests.InfrastructureTests.ServicesTests
{
    public class AnalyticsServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetSalesAnalysisAsync_ReturnsOnlyCurrentUserSales()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var AnalyticsRepo = new AnalyticsService(context, currentUserMock.Object);

            context.Sales.AddRange(
                new Sale { SaleId = 1, UserId = 7, SaleDate = new DateTime(2024, 1, 10), TotalAmount = 100 },
                new Sale { SaleId = 2, UserId = 5, SaleDate = new DateTime(2024, 1, 10), TotalAmount = 200 }
            );

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await AnalyticsRepo.GetSalesAnalysisAsync();

            result.Should().HaveCount(1);
            result.Single().Revenue.Should().Be(100);
        }

        [Fact]
        public async Task GetSalesAnalysisAsync_GroupsByYearAndMonth()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var AnalyticsRepo = new AnalyticsService(context, currentUserMock.Object);

            context.Sales.AddRange(
                new Sale { SaleId = 1, UserId = 7, SaleDate = new DateTime(2024, 1, 10), TotalAmount = 100 },
                new Sale { SaleId = 2, UserId = 7, SaleDate = new DateTime(2024, 1, 20), TotalAmount = 150 },
                new Sale { SaleId = 3, UserId = 7, SaleDate = new DateTime(2024, 2, 5), TotalAmount = 200 }
            );

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await AnalyticsRepo.GetSalesAnalysisAsync();

            result.Should().HaveCount(2);

            var jan = result.First(x => x.Month == 1);
            jan.Revenue.Should().Be(250);

            var feb = result.First(x => x.Month == 2);
            feb.Revenue.Should().Be(200);
        }

        [Fact]
        public async Task GetSalesAnalysisAsync_OrdersByYearThenMonth()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var AnalyticsRepo = new AnalyticsService(context, currentUserMock.Object);

            context.Sales.AddRange(
                new Sale { SaleId = 1, UserId = 7, SaleDate = new DateTime(2025, 5, 10), TotalAmount = 100 },
                new Sale { SaleId = 2, UserId = 7, SaleDate = new DateTime(2024, 12, 10), TotalAmount = 200 },
                new Sale { SaleId = 3, UserId = 7, SaleDate = new DateTime(2025, 1, 10), TotalAmount = 300 }
            );

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await AnalyticsRepo.GetSalesAnalysisAsync();

            result.Select(r => (r.Year, r.Month)).Should().BeEquivalentTo(
                new[]
                {
            (2025, 5),
            (2025, 1),
            (2024, 12)
                },
                options => options.WithStrictOrdering()
            );
        }

    }
}
