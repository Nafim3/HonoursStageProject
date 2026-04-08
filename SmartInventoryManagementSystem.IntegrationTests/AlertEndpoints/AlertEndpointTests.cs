using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SmartInventoryManagementSystem.ApiTests.Helper;
using SmartInventoryManagementSystem.Application.DTO.NotificationDTO;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.ApiTests.AlertEndpoint
{
    public class AlertEndpointTests
    {
        [Fact]
        public async Task DeleteAlert_ShouldReturnNotFound_WhenAlertDoesNotExist()
        {
            await using var factory = new ApiFactory();
            var client = factory.CreateClient();

          
            var response = await client.DeleteAsync("/api/alert/1");

         
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteAlert_ShouldReturnNotFound_WhenAlertBelongsToAnotherUser()
        {
            await using var factory = new ApiFactory();
            var client = factory.CreateClient();

            
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Alerts.Add(new Alert
                {
                    Id = 1,
                    UserId = 54,
                    Message = "Test alert"
                });
                db.SaveChanges();
            }

         
            var response = await client.DeleteAsync("/api/alert/1");

        
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteAlert_ShouldReturnOk_WhenSuccessful()
        {
            await using var factory = new ApiFactory();
            var client = factory.CreateClient();

          
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Alerts.RemoveRange(db.Alerts);
                await db.SaveChangesAsync();

                await db.Alerts.AddAsync(new Alert
                {
                    Id = 1,
                    UserId = 1,
                    Message = "Test alert"
                });

                await db.SaveChangesAsync();
            }

          
            var response = await client.DeleteAsync("/api/alert/1");

           
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllAlerts_ShouldReturnUserAlerts()
        {
            await using var factory = new ApiFactory();
            var client = factory.CreateClient();

          
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Alerts.RemoveRange(db.Alerts);
                await db.SaveChangesAsync();

                await db.Alerts.AddRangeAsync(
                    new Alert
                    {
                        Id = 1,
                        UserId = 1,
                        Message = "User 1 Alert",
                        Category = "Stock"
                    },
                    new Alert
                    {
                        Id = 2,
                        UserId = 1,
                        Message = "Another User 1 Alert",
                        Category = "Stock"
                    },
                    new Alert
                    {
                        Id = 3,
                        UserId = 2,
                        Message = "User 2 Alert",
                        Category = "Stock"
                    }
                );

                await db.SaveChangesAsync();
            }

    
            var response = await client.GetAsync("/api/alert/list");

         
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            
            var content = await response.Content.ReadAsStringAsync();
            var alerts = JsonSerializer.Deserialize<List<AlertDTO>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            alerts.Should().NotBeNull();
            alerts!.Count.Should().Be(2); 
        }
    }
}
