using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Infrastructure.Repositories;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartInventoryManagementSystem.Application.Interfaces;
using Moq;
using FluentAssertions;

namespace SmartInventoryManagementSystem.UnitTests.InfrastructureTests.RepositoryTests
{
    public class ProductRepositoryTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetProducts_ShouldReturn_ThecorrectproductwiththecorrectUserID()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            context.Products.AddRange(
                new Product { ProductId = 37, ProductName = "Milk", UserId = 1, IsActive = true },
                new Product { ProductId = 4, ProductName = "Bread", UserId = 7, IsActive = true },
                new Product { ProductId = 19, ProductName = "Sugar", UserId = 7, IsActive = true },
                new Product { ProductId = 13, ProductName = "Wheat", UserId = 7, IsActive = false }
            );

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await Prepository.GetProductAsync();

            result.Should().HaveCount(2);
            result.Select(p => p.ProductName).Should().Contain(new[] { "Bread", "Sugar" });


        }

        [Fact]
        public async Task GetProducts_ShouldIgnoreInactiveProducts()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            context.Products.Add(
                new Product { ProductId = 13, ProductName = "Wheat", UserId = 7, IsActive = false }
            );

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await Prepository.GetProductAsync();

            result.Should().BeEmpty();
        }



        [Fact]
        public async Task AddProductAsync_ShouldReturnError_WhenProductIsNull()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(5);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var result = await Prepository.AddProductAsync(null);

            result.Should().Be("Product is null");
        }

        [Fact]
        public async Task AddProductAsync_ShouldReturnError_WhenExpiryDateIsNotInFuture()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(5);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var product = new Product
            {
                ProductName = "Milk",
                ExpiryDate = DateTime.Today, // invalid
                IsActive = true
            };

            var result = await Prepository.AddProductAsync(product);

            result.Should().Be("Expiry date must be in the future");
        }

        [Fact]
        public async Task AddProductAsync_ShouldAddProduct_WhenValid()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var product = new Product
            {
                ProductName = "Bread",
                ExpiryDate = DateTime.Today.AddDays(5),
                IsActive = true
            };

            var result = await Prepository.AddProductAsync(product);

            result.Should().BeNull();

            var saved = await context.Products.FirstOrDefaultAsync(p => p.ProductName == "Bread");

            saved.Should().NotBeNull();
            saved.UserId.Should().Be(7);
            saved.ExpiryDate.Should().Be(product.ExpiryDate);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenAllConditionsMatch()
        {
            var context = GetInMemoryDbContext();

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var product = new Product
            {
                ProductId = 10,
                ProductName = "Bread",
                UserId = 7,
                IsActive = true
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var result = await Prepository.GetProductByIdAsync(10);

            result.Should().NotBeNull();
            result!.ProductName.Should().Be("Bread");
            result.UserId.Should().Be(7);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenUserDoesNotMatch()
        {
            var context = GetInMemoryDbContext();

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            context.Products.Add(new Product
            {
                ProductId = 10,
                ProductName = "Peanuts",
                
                UserId = 1, // different user
                IsActive = true
            });

            await context.SaveChangesAsync();

            var result = await Prepository.GetProductByIdAsync(10);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductIsInactive()
        {
            var context = GetInMemoryDbContext();

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            context.Products.Add(new Product
            {
                ProductId = 10,
                ProductName = "Cabbage",
                UserId = 7,
                IsActive = false     // inactive
            });

            await context.SaveChangesAsync();

            var result = await Prepository.GetProductByIdAsync(10);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldReturnError_WhenProductIsNull()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var result = await Prepository.UpdateProductAsync(null);

            result.Should().Be("Product is null");
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldReturnError_WhenProductNotFound()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var updated = new Product
            {
                ProductId = 1298, // does not exist
                ProductName = "Tomato",
                ExpiryDate = DateTime.Today.AddDays(5)
            };

            var result = await Prepository.UpdateProductAsync(updated);

            result.Should().Be("Product not found");
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldReturnError_WhenExpiryDateInvalid()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var existing = new Product
            {
                ProductId = 10,
                ProductName = "Oranges",
                UserId = 7,
                IsActive = true,
                ExpiryDate = DateTime.Today.AddDays(10)
            };

            context.Products.Add(existing);
            await context.SaveChangesAsync();

            var updated = new Product
            {
                ProductId = 10,
                ProductName = "Oranges Updated",
                
                ExpiryDate = DateTime.Today // invalid
            };

            var result = await Prepository.UpdateProductAsync(updated);

            result.Should().Be("Expiry date must be in the future");
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldUpdateProduct_WhenValid()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var existing = new Product
            {
                ProductId = 10,
                ProductName = "Almond",
                QuantityInStock = 5,
                ReorderLevel = 2,
                ProductPrice = 1.50m,
                ExpiryDate = DateTime.Today.AddDays(10),
                UserId = 7,
                IsActive = true
            };

            context.Products.Add(existing);
            await context.SaveChangesAsync();

            var updated = new Product
            {
                ProductId = 10,
                ProductName = "Almond Updated",
                QuantityInStock = 20,
                ReorderLevel = 5,
                ProductPrice = 2.99m,
                ExpiryDate = DateTime.Today.AddDays(20)
            };

            var result = await Prepository.UpdateProductAsync(updated);

            result.Should().BeNull();

            var saved = await context.Products.FirstAsync(p => p.ProductId == 10);

            saved.ProductName.Should().Be("Almond Updated");
            saved.QuantityInStock.Should().Be(20);
            saved.ReorderLevel.Should().Be(5);
            saved.ProductPrice.Should().Be(2.99m);
            saved.ExpiryDate.Should().Be(updated.ExpiryDate);
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldReturnError_WhenProductNotFound()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var result = await Prepository.DeleteProductAsync(999);

            result.Should().Be("Product not found");
        }


        [Fact]
        public async Task DeleteProductAsync_ShouldSoftDelete_WhenProductHasSaleItems()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var product = new Product
            {
                ProductId = 10,
                ProductName = "Onion",
                UserId = 7,
                IsActive = true,
                SaleItems = new List<SaleItem>
        {
            new SaleItem { SaleItemId = 1, Quantity = 1 }
        }
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var result = await Prepository.DeleteProductAsync(10);

            result.Should().BeNull();

            var saved = await context.Products.IgnoreQueryFilters().FirstAsync(p => p.ProductId == 10);


            saved.IsActive.Should().BeFalse();      // soft delete
            saved.SaleItems.Should().NotBeEmpty();  // still exists in DB
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldHardDelete_WhenNoSaleItems()
        {
            var context = GetInMemoryDbContext();
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            var product = new Product
            {
                ProductId = 10,
                ProductName = "Plate",
                UserId = 7,
                IsActive = true,
                SaleItems = new List<SaleItem>() // empty
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var result = await Prepository.DeleteProductAsync(10);

            result.Should().BeNull();

            var exists = await context.Products.AnyAsync(p => p.ProductId == 10);
            
            exists.Should().BeFalse(); // hard delete
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnLowStockProducts_ForCurrentUser()
        {
            var context = GetInMemoryDbContext();

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            context.Products.AddRange(
                new Product { ProductId = 1, ProductName = "Milk", UserId = 7, QuantityInStock = 2, ReorderLevel = 5, IsActive = true },
                new Product { ProductId = 2, ProductName = "Bread", UserId = 7, QuantityInStock = 10, ReorderLevel = 5, IsActive = true },
                new Product { ProductId = 3, ProductName = "Sugar", UserId = 1, QuantityInStock = 1, ReorderLevel = 5, IsActive = true }
            );

            await context.SaveChangesAsync();

            var result = await Prepository.GetLowStockProductsAsync(7);

            result.Count.Should().Be(1);
            result.First().ProductName.Should().Be("Milk");
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnEmptyList_WhenNoLowStockProducts()
        {
            var context = GetInMemoryDbContext();

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            context.Products.AddRange(
                new Product { ProductId = 1, ProductName = "Milk", UserId = 7, QuantityInStock = 10, ReorderLevel = 5, IsActive = true },
                new Product { ProductId = 2, ProductName = "Bread", UserId = 7, QuantityInStock = 20, ReorderLevel = 5, IsActive = true }
            );

            await context.SaveChangesAsync();

            var result = await Prepository.GetLowStockProductsAsync(7);

            result.Should().BeEmpty();
        }


        [Fact]
        public async Task GetExpiredProductsAsync_ShouldReturnExpiredProducts_ForCurrentUser()
        {
            var context = GetInMemoryDbContext();

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            context.Products.AddRange(
                new Product { ProductId = 1, ProductName = "Milk", UserId = 7, ExpiryDate = DateTime.UtcNow.AddDays(-1), IsActive = true },
                new Product { ProductId = 2, ProductName = "Bread", UserId = 7, ExpiryDate = DateTime.UtcNow.AddDays(5), IsActive = true },
                new Product { ProductId = 3, ProductName = "Sugar", UserId = 1, ExpiryDate = DateTime.UtcNow.AddDays(-2), IsActive = true }
            );

            await context.SaveChangesAsync();

            var result = await Prepository.GetExpiredProductsAsync(7);

            result.Count.Should().Be(1);
            result.First().ProductName.Should().Be("Milk");
        }

        [Fact]
        public async Task GetExpiredProductsAsync_ShouldReturnEmptyList_WhenNoExpiredProducts()
        {
            var context = GetInMemoryDbContext();

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            context.Products.AddRange(
                new Product { ProductId = 1, ProductName = "Milk", UserId = 7, ExpiryDate = DateTime.UtcNow.AddDays(3), IsActive = true },
                new Product { ProductId = 2, ProductName = "Bread", UserId = 7, ExpiryDate = DateTime.UtcNow.AddDays(10), IsActive = true }
            );

            await context.SaveChangesAsync();

            var result = await Prepository.GetExpiredProductsAsync(7);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetExpiredProductsAsync_ShouldIgnoreProductsFromOtherUsers()
        {
            var context = GetInMemoryDbContext();

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.UserId).Returns(7);

            var Prepository = new ProductRepository(context, currentUserMock.Object);

            context.Products.Add(
                new Product { ProductId = 1, ProductName = "Milk", UserId = 1, ExpiryDate = DateTime.UtcNow.AddDays(-1), IsActive = true }
            );

            await context.SaveChangesAsync();

            var result = await Prepository.GetExpiredProductsAsync(7);

            result.Should().BeEmpty();
        }

       


    }
}
