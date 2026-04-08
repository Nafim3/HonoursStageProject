using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartInventoryManagementSystem.Application.DTO.SaleDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.UnitTests.InfrastructureTests.ServicesTests
{
    public class SaleServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateSaleAsync_Throws_WhenNoItems()
        {
            var db = GetInMemoryDbContext();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Jamal",
                Items = new List<CreateSaleItemRequest>()

            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateSaleAsync(request));
        }

        [Fact]
        public async Task CreateSaleAsync_Throws_WhenDuplicateProducts()
        {
            var db = GetInMemoryDbContext();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Piyal",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 10, Quantity = 1 },
            new() { ProductId = 10, Quantity = 2 }
        }
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateSaleAsync(request));
        }

        [Fact]
        public async Task CreateSaleAsync_Throws_WhenProductNotFound()
        {
            var db = GetInMemoryDbContext();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Siam",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 99, Quantity = 1 }
        }
            };

            await Assert.ThrowsAsync<Exception>(() => service.CreateSaleAsync(request));
        }

        [Fact]
        public async Task CreateSaleAsync_Throws_WhenInsufficientStock()
        {
            var db = GetInMemoryDbContext();

            db.Products.Add(new Product
            {
                ProductId = 10,
                UserId = 1,
                ProductName = "Keyboard",
                QuantityInStock = 1,
                ProductPrice = 50
            });
            await db.SaveChangesAsync();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Adnan",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 10, Quantity = 5 }
        }
            };

            await Assert.ThrowsAsync<Exception>(() => service.CreateSaleAsync(request));
        }

        [Fact]
        public async Task CreateSaleAsync_CreatesSale_AndDeductsStock()
        {
            var db = GetInMemoryDbContext();

            db.Products.Add(new Product
            {
                ProductId = 10,
                UserId = 1,
                ProductName = "Keyboard",
                QuantityInStock = 10,
                ProductPrice = 50,
                ReorderLevel = 2
            });
            await db.SaveChangesAsync();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Rakib",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 10, Quantity = 3 }
        }
            };

            var result = await service.CreateSaleAsync(request);

            result.TotalAmount.Should().Be(150);
            result.ItemCount.Should().Be(1);

            var product = await db.Products.FindAsync(10);
            product!.QuantityInStock.Should().Be(7);
        }

        [Fact]
        public async Task CreateSaleAsync_CallsLowStockNotification_WhenStockLow()
        {
            var db = GetInMemoryDbContext();

            db.Products.Add(new Product
            {
                ProductId = 10,
                UserId = 1,
                ProductName = "Keyboard",
                QuantityInStock = 3,
                ProductPrice = 50,
                ReorderLevel = 3
            });
            await db.SaveChangesAsync();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

         
            var service = new SaleService(
                db,
                currentUser.Object,
                notification.Object,
                disableTransactions: true
            );

            var request = new CreateSaleRequest
            {
                BuyerName = "Anik",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 10, Quantity = 1 }
        }
            };

            await service.CreateSaleAsync(request);

            notification.Verify(
                n => n.NotifyLowStockAsync(It.IsAny<Product>(), 1),
                Times.Once
            );

        }

        [Fact]
        public async Task CreateSaleAsync_CalculatesTotalCorrectly()
        {
            var db = GetInMemoryDbContext();

            db.Products.AddRange(
                new Product { ProductId = 1, UserId = 1, ProductName = "A", QuantityInStock = 10, ProductPrice = 10 },
                new Product { ProductId = 2, UserId = 1, ProductName = "B", QuantityInStock = 10, ProductPrice = 5 }
            );
            await db.SaveChangesAsync();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Akib",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 1, Quantity = 2 },
            new() { ProductId = 2, Quantity = 3 }  
        }
            };

            var result = await service.CreateSaleAsync(request);

            Assert.Equal(35, result.TotalAmount);
        }


        [Fact]
        public async Task CreateSaleAsync_DeductsStockCorrectly()
        {
            var db = GetInMemoryDbContext();

            db.Products.Add(new Product
            {
                ProductId = 10,
                UserId = 1,
                ProductName = "Keyboard",
                QuantityInStock = 10,
                ProductPrice = 50
            });
            await db.SaveChangesAsync();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Khaled",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 10, Quantity = 3 }
        }
            };

            await service.CreateSaleAsync(request);

            var product = db.Products.First(p => p.ProductId == 10);
            Assert.Equal(7, product.QuantityInStock);
        }

        [Fact]
        public async Task CreateSaleAsync_Throws_WhenProductExpired()
        {
            var db = GetInMemoryDbContext();

            db.Products.Add(new Product
            {
                ProductId = 10,
                UserId = 1,
                ProductName = "Milk",
                QuantityInStock = 10,
                ProductPrice = 2,
                ExpiryDate = DateTime.UtcNow.AddDays(-1) // expired
            });
            await db.SaveChangesAsync();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Rishad",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 10, Quantity = 1 }
        }
            };

            await Assert.ThrowsAsync<Exception>(() => service.CreateSaleAsync(request));
        }

        [Fact]
        public async Task CreateSaleAsync_DoesNotCallLowStockNotification_WhenStockAboveReorder()
        {
            var db = GetInMemoryDbContext();

            db.Products.Add(new Product
            {
                ProductId = 10,
                UserId = 1,
                ProductName = "Keyboard",
                QuantityInStock = 10,
                ProductPrice = 50,
                ReorderLevel = 3
            });
            await db.SaveChangesAsync();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Raisul",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 10, Quantity = 1 }
        }
            };

            await service.CreateSaleAsync(request);

            notification.Verify(n => n.NotifyLowStockAsync(It.IsAny<Product>(), 1), Times.Never);
        }

        [Fact]
        public async Task CreateSaleAsync_Throws_WhenProductBelongsToAnotherUser()
        {
            var db = GetInMemoryDbContext();

            db.Products.Add(new Product
            {
                ProductId = 10,
                UserId = 67,
                ProductName = "Keyboard",
                QuantityInStock = 10,
                ProductPrice = 50
            });
            await db.SaveChangesAsync();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Nafiz",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 10, Quantity = 1 }
        }
            };

            await Assert.ThrowsAsync<Exception>(() => service.CreateSaleAsync(request));
        }

        [Fact]
        public async Task CreateSaleAsync_CreatesSaleAndSaleItems()
        {
            var db = GetInMemoryDbContext();

            db.Products.Add(new Product
            {
                ProductId = 10,
                UserId = 1,
                ProductName = "Keyboard",
                QuantityInStock = 10,
                ProductPrice = 50
            });
            await db.SaveChangesAsync();

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.UserId).Returns(1);

            var notification = new Mock<INotificationService>();

            var service = new SaleService(db, currentUser.Object, notification.Object, disableTransactions: true);

            var request = new CreateSaleRequest
            {
                BuyerName = "Jawad",
                Items = new List<CreateSaleItemRequest>
        {
            new() { ProductId = 10, Quantity = 1 }
        }
            };

            var result = await service.CreateSaleAsync(request);

            Assert.True(db.Sales.Any(s => s.SaleId == result.SaleId));
            Assert.True(db.SaleItems.Any(si => si.SaleId == result.SaleId));
        }


    }
}
