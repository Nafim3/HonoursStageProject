using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.UnitTests.InfrastructureTests.RepositoryTests
{
    public class SaleRepositoryTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task FetchAllSalesAsync_ReturnsSalesForCurrentUser()
        {

            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(5);

            var SlrepoSitory = new SaleRepository(context, currentUserMock.Object);

            context.Sales.AddRange(
                new Sale { SaleId = 1, UserId = 5 },
                new Sale { SaleId = 2, UserId = 7 },
                new Sale { SaleId = 3, UserId = 5 } 
            );

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await SlrepoSitory.FetchAllSalesAsync();

            result.Should().HaveCount(2);
            result.Select(s => s.SaleId).Should().Contain(new[] { 1, 3 });

        }

        [Fact]
        public async Task FetchAllSalesAsync_ShouldIncludeSaleItems_AndProducts()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var SlrepoSitory = new SaleRepository(context, currentUserMock.Object);

            var product = new Product
            {
                ProductId = 10,
                ProductName = "Milk",
                UserId = 7,
                IsActive = true
            };

            var sale = new Sale
            {
                SaleId = 1,
                UserId = 7,
                SaleItems = new List<SaleItem>
        {
            new SaleItem { SaleItemId = 1, Product = product, Quantity = 2 }
        }
            };

            context.Sales.Add(sale);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await SlrepoSitory.FetchAllSalesAsync();

            var returnedSale = result.Single();
            returnedSale.SaleItems.Should().HaveCount(1);


            var loadedProduct = returnedSale.SaleItems.First().Product;

            loadedProduct.Should().NotBeNull();
            loadedProduct!.ProductName.Should().Be("Milk");

        }

        [Fact]
        public async Task FetchSaleByIdAsync_ReturnsSaleForCurrentUser()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var SlrepoSitory = new SaleRepository(context, currentUserMock.Object);

            context.Sales.AddRange(
                new Sale { SaleId = 1, UserId = 7 },
                new Sale { SaleId = 2, UserId = 5 }
            );

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await SlrepoSitory.FetchSaleByIdAsync(1);

            result.Should().NotBeNull();
            result!.SaleId.Should().Be(1);
        }

        [Fact]
        public async Task FetchSaleByIdAsync_ReturnsNull_WhenSaleDoesNotBelongToUser()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var SlrepoSitory = new SaleRepository(context, currentUserMock.Object);

            context.Sales.Add(new Sale { SaleId = 1, UserId = 5 });

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await SlrepoSitory.FetchSaleByIdAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task FetchSaleByIdAsync_ShouldIncludeSaleItems_AndProducts()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var SlrepoSitory = new SaleRepository(context, currentUserMock.Object);

            var product = new Product
            {
                ProductId = 10,
                ProductName = "Milk",
                UserId = 7,
                IsActive = true
            };

            var sale = new Sale
            {
                SaleId = 1,
                UserId = 7,
                SaleItems = new List<SaleItem>
        {
            new SaleItem { SaleItemId = 1, Product = product, Quantity = 2 }
        }
            };

            context.Sales.Add(sale);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await SlrepoSitory.FetchSaleByIdAsync(1);

            result.Should().NotBeNull();
            result!.SaleItems.Should().HaveCount(1);

            var loadedProduct = result.SaleItems.First().Product;
            loadedProduct.Should().NotBeNull();
            loadedProduct!.ProductName.Should().Be("Milk");
        }


    }
}
