using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartInventoryManagementSystem.ApiTests.Helper;
using SmartInventoryManagementSystem.Application.DTO.DashboardDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.ApiTests.DashboardEndpoints
{
    public class DashboardEndpointTests
    {
        [Fact]
        public async Task GetDashboard_ShouldReturnDashboardData_WhenUserIsAuthenticated()
        {
            await using var factory = new ApiFactory();

            var mockProductRepo = new Mock<IProductRepository>();
            var mockSaleRepo = new Mock<ISaleRepository>();
            var mockAlertRepo = new Mock<IAlertRepository>();

            mockProductRepo.Setup(r => r.GetProductAsync())
                .ReturnsAsync(new List<Product> { new Product(), new Product() }); // 2 products

            mockSaleRepo.Setup(r => r.FetchAllSalesAsync())
                .ReturnsAsync(new List<Sale>
                {
            new Sale { SaleDate = DateTime.UtcNow },
            new Sale { SaleDate = DateTime.UtcNow.AddDays(-1) }
                });

            mockAlertRepo.Setup(r => r.CountAsync(1, "Low Stock")).ReturnsAsync(3);
            mockAlertRepo.Setup(r => r.CountAsync(1, "Expired")).ReturnsAsync(1);

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ISaleRepository>();
                    services.RemoveAll<IAlertRepository>();

                    services.AddScoped(_ => mockProductRepo.Object);
                    services.AddScoped(_ => mockSaleRepo.Object);
                    services.AddScoped(_ => mockAlertRepo.Object);
                });
            }).CreateClient();

            var response = await client.GetAsync("/api/dashboard");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<Dashboard>();

            dto!.TotalProducts.Should().Be(2);
            dto.SalesToday.Should().Be(1);
            dto.LowStockCount.Should().Be(3);
            dto.ExpiredProductsCount.Should().Be(1);
        }

        [Fact]
        public async Task GetDashboard_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            await using var factory = new ApiFactory();

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthenticationSchemeProvider>();
                    services.RemoveAll<IAuthenticationHandlerProvider>();

                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "None";
                        options.DefaultChallengeScheme = "None";
                    })
                    .AddScheme<AuthenticationSchemeOptions, NoAuthHandler>("None", _ => { });
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = false,
                AllowAutoRedirect = false
            });

            var response = await client.GetAsync("/api/dashboard");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private class NoAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public NoAuthHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder)
                : base(options, logger, encoder) { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
                => Task.FromResult(AuthenticateResult.Fail("No auth"));
        }


    }
}
