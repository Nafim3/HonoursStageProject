using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
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
    public class NotificationTests
    {
        [Fact]
        public async Task NotifyLowStockAsync_DoesNothing_WhenAlertAlreadyExists()
        {
            // Arrange
            var alertRepoMock = new Mock<IAlertRepository>();
            var NotificationServiceTest = new NotificationService(alertRepoMock.Object);

            var product = new Product
            {
                ProductId = 10,
                ProductName = "Keyboard",
                QuantityInStock = 2
            };

            alertRepoMock
                .Setup(r => r.ExistAsync(5, "Low Stock", 10))
                .ReturnsAsync(true);

       
            await NotificationServiceTest.NotifyLowStockAsync(product, 5);

         
            alertRepoMock.Verify(r => r.ExistAsync(5, "Low Stock", 10), Times.Once);
            alertRepoMock.Verify(r => r.AddAsync(It.IsAny<Alert>()), Times.Never);
        }

        [Fact]
        public async Task NotifyLowStockAsync_CreatesAlert_WhenNoneExists()
        {
      
            var alertRepoMock = new Mock<IAlertRepository>();
            var NotificationServiceTest = new NotificationService(alertRepoMock.Object);

            var product = new Product
            {
                ProductId = 10,
                ProductName = "Keyboard",
                QuantityInStock = 2
            };

            alertRepoMock
                .Setup(r => r.ExistAsync(5, "Low Stock", 10))
                .ReturnsAsync(false);

            Alert? capturedAlert = null;

            alertRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Alert>()))
                .Callback<Alert>(a => capturedAlert = a)
                .Returns(Task.CompletedTask);

   
            await NotificationServiceTest.NotifyLowStockAsync(product, 5);

      
            alertRepoMock.Verify(r => r.ExistAsync(5, "Low Stock", 10), Times.Once);
            alertRepoMock.Verify(r => r.AddAsync(It.IsAny<Alert>()), Times.Once);

            capturedAlert.Should().NotBeNull();
            capturedAlert!.Category.Should().Be("Low Stock");
            capturedAlert.Message.Should().Be("Keyboard is low in stock (Quantity: 2)");
            capturedAlert.UserId.Should().Be(5);
            capturedAlert.ProductId.Should().Be(10);
        }

        [Fact]
        public async Task NotifyExpiredProductsAsync_DoesNothing_WhenAlertAlreadyExists()
        {
  
            var alertRepoMock = new Mock<IAlertRepository>();
            var NotificationServiceTest = new NotificationService(alertRepoMock.Object);

            var product = new Product
            {
                ProductId = 10,
                ProductName = "Milk",
                ExpiryDate = new DateTime(2024, 1, 1)
            };

            alertRepoMock
                .Setup(r => r.ExistAsync(5, "Expired", 10))
                .ReturnsAsync(true);

     
            await NotificationServiceTest.NotifyExpiredProductsAsync(product, 5);

          
            alertRepoMock.Verify(r => r.ExistAsync(5, "Expired", 10), Times.Once);
            alertRepoMock.Verify(r => r.AddAsync(It.IsAny<Alert>()), Times.Never);
        }

        [Fact]
        public async Task NotifyExpiredProductsAsync_CreatesAlert_WhenNoneExists()
        {
    
            var alertRepoMock = new Mock<IAlertRepository>();
            var NotificationServiceTest = new NotificationService(alertRepoMock.Object);

            var product = new Product
            {
                ProductId = 10,
                ProductName = "Milk",
                ExpiryDate = new DateTime(2024, 1, 1)
            };

            alertRepoMock
                .Setup(r => r.ExistAsync(5, "Expired", 10))
                .ReturnsAsync(false);

            Alert? capturedAlert = null;

            alertRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Alert>()))
                .Callback<Alert>(a => capturedAlert = a)
                .Returns(Task.CompletedTask);

           
            await NotificationServiceTest.NotifyExpiredProductsAsync(product, 5);

          
            alertRepoMock.Verify(r => r.ExistAsync(5, "Expired", 10), Times.Once);
            alertRepoMock.Verify(r => r.AddAsync(It.IsAny<Alert>()), Times.Once);

            capturedAlert.Should().NotBeNull();
            capturedAlert!.Category.Should().Be("Expired");
            capturedAlert.Message.Should().Be("Milk expired on 01/01/2024");
            capturedAlert.UserId.Should().Be(5);
            capturedAlert.ProductId.Should().Be(10);
        }

        [Fact]
        public async Task GetBellAlertsAsync_ReturnsCombinedSortedAndMappedAlerts()
        {
            
            var alertRepoMock = new Mock<IAlertRepository>();
            var NotificationServiceTest = new NotificationService(alertRepoMock.Object);

            var expiredAlerts = new List<Alert>
    {
        new Alert { Id = 1, Category = "Expired", Message = "Expired A", CreatedAt = new DateTime(2024, 1, 2) },
        new Alert { Id = 2, Category = "Expired", Message = "Expired B", CreatedAt = new DateTime(2024, 1, 1) }
    };

            var lowStockAlerts = new List<Alert>
    {
        new Alert { Id = 3, Category = "Low Stock", Message = "LowStock A", CreatedAt = new DateTime(2024, 1, 3) }
    };

            alertRepoMock
                .Setup(r => r.GetRecentAsync(5, "Expired", 5))
                .ReturnsAsync(expiredAlerts);

            alertRepoMock
                .Setup(r => r.GetRecentAsync(5, "Low Stock", 5))
                .ReturnsAsync(lowStockAlerts);


            var result = await NotificationServiceTest.GetBellAlertsAsync(5);

    
            alertRepoMock.Verify(r => r.GetRecentAsync(5, "Expired", 5), Times.Once);
            alertRepoMock.Verify(r => r.GetRecentAsync(5, "Low Stock", 5), Times.Once);

            result.Should().HaveCount(3);

            // Sorted descending by CreatedAt
            result[0].Id.Should().Be(3); // 2024-01-03
            result[1].Id.Should().Be(1); // 2024-01-02
            result[2].Id.Should().Be(2); // 2024-01-01

          
            result[0].Category.Should().Be("Low Stock");
            result[0].Message.Should().Be("LowStock A");
        }


    }
}
