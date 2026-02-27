using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
    public class AlertRepositoryTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddAlertToDatabase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new AlertRepository(context);

            var alert = new Alert
            {
                UserId = 1,
                Category = "Stock",
                Message = "Low stock for product",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await repository.AddAsync(alert);

            // Assert
            var alertsInDb = await context.Alerts.ToListAsync();
            alertsInDb.Count.Should().Be(1);
            alertsInDb.First().Message.Should().Be("Low stock for product");
        }

        [Fact]
        public async Task GetRecentAsync_ShouldReturnCorrectAlerts()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);

            var alert1 = new Alert { UserId = 1, Category = "Stock", Message = "Alert 1", CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
            var alert2 = new Alert { UserId = 1, Category = "Stock", Message = "Alert 2", CreatedAt = DateTime.UtcNow };
            var alert3 = new Alert { UserId = 2, Category = "Stock", Message = "Alert 3", CreatedAt = DateTime.UtcNow };

            context.Alerts.AddRange(alert1, alert2, alert3);
            await context.SaveChangesAsync();

            // Act
            var recentAlerts = await Arepository.GetRecentAsync(userId: 1, category: "Stock", limit: 2);

            // Assert
            recentAlerts.Count.Should().Be(2);
            recentAlerts[0].Message.Should().Be("Alert 2"); // most recent first
            recentAlerts[1].Message.Should().Be("Alert 1");
        }

        [Fact]
        public async Task ExistAsync_ShouldReturnTrue_WhenAlertExists()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);

            var alert = new Alert
            {
                UserId = 1,
                Category = "Stock",
                Message = "Alert 1",
                CreatedAt = DateTime.UtcNow,
                ProductId = 4
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var exists = await Arepository.ExistAsync(1, "Stock", 4);
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistAsync_ShouldReturnFalse_WhenAlertDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);
            var alert = new Alert
            {
                UserId = 1,
                Category = "Stock",
                Message = "Alert 1",
                CreatedAt = DateTime.UtcNow,
                ProductId = 4
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();
            var exists = await Arepository.ExistAsync(1, "Stock", 5);
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CountAsync_ShouldReturnCorrectCount()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);
            context.Alerts.AddRange
            (
                new Alert { UserId = 1, Category = "Stock", Message = "Alert 1", CreatedAt = DateTime.UtcNow },
                new Alert { UserId = 1, Category = "Stock", Message = "Alert 2", CreatedAt = DateTime.UtcNow },
                new Alert { UserId = 1, Category = "Other", Message = "Alert 3", CreatedAt = DateTime.UtcNow },
                new Alert { UserId = 2, Category = "Stock", Message = "Alert 4", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
            var count = await Arepository.CountAsync(1, "Stock");
            count.Should().Be(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectAlert()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);
            var alert = new Alert
            {
                UserId = 12,
                Category = "Expired",
                Message = "Alert 1",
                CreatedAt = DateTime.UtcNow,
                ProductId = 7
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();
            var retrievedAlert = await Arepository.GetByIdAsync(alert.Id);
            retrievedAlert.Should().NotBeNull();
            retrievedAlert!.Message.Should().Be("Alert 1");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenAlertDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);
            var retrievedAlert = await Arepository.GetByIdAsync(877);
            retrievedAlert.Should().BeNull();
        }

        [Fact]
        public async Task GetRecentAsync_ShouldReturnEmptyList_WhenNoAlerts()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);
            var recentAlerts = await Arepository.GetRecentAsync(1, "Stock", 5);
            recentAlerts.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteAsync_Should_RemoveAlertFromDatabase()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);
            var alert = new Alert
            {
                UserId = 1,
                Category = "Expired",
                Message = "Alert to delete",
                CreatedAt = DateTime.UtcNow,
                ProductId = 3
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            context.Alerts.Remove(alert);
            await context.SaveChangesAsync();

            var alertsInDb = await context.Alerts.ToListAsync();
            alertsInDb.Should().BeEmpty();

        }

        [Fact]
        public async Task GetAllForUserAsync_ShouldReturnAlertsForUser()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);
            context.Alerts.AddRange
            (
                new Alert { UserId = 1, Category = "Stock", Message = "Alert 1", CreatedAt = DateTime.UtcNow },
                new Alert { UserId = 1, Category = "Stock", Message = "Alert 2", CreatedAt = DateTime.UtcNow },
                new Alert { UserId = 2, Category = "Stock", Message = "Alert 3", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                new Alert { UserId = 2, Category = "Expired", Message = "Alert 4", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
            var user1Alerts = await Arepository.GetAllForUserAsync(2);
            user1Alerts.Count.Should().Be(2);
            user1Alerts[0].Message.Should().Be("Alert 4");
            user1Alerts[1].Message.Should().Be("Alert 3");
        }

        [Fact]
        public async Task GetRecentAsync_ShouldReturnMostRecentAlerts()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);

            var older = new Alert
            {
                UserId = 1,
                Category = "Stock",
                Message = "Old Alert",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var middle = new Alert
            {
                UserId = 1,
                Category = "Stock",
                Message = "Middle Alert",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            var newest = new Alert
            {
                UserId = 1,
                Category = "Stock",
                Message = "New Alert",
                CreatedAt = DateTime.UtcNow
            };

            context.Alerts.AddRange(older, middle, newest);
            await context.SaveChangesAsync();

            var result = await Arepository.GetRecentAsync(1, "Stock", 2);

            result.Count.Should().Be(2);
            result[0].Message.Should().Be("New Alert");     // newest
            result[1].Message.Should().Be("Middle Alert");  // second newest
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveAlert()
        {
            var context = GetInMemoryDbContext();
            var Arepository = new AlertRepository(context);

            var alert = new Alert
            {
                UserId = 1,
                Category = "Stock",
                Message = "To be deleted",
                CreatedAt = DateTime.UtcNow
            };

            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            // Act
            await Arepository.DeleteAsync(alert);

            // Assert
            var exists = await context.Alerts.AnyAsync(a => a.Id == alert.Id);
            exists.Should().BeFalse();
        }

    }
}   