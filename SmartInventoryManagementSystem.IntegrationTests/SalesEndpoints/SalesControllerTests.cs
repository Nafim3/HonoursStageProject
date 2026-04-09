using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartInventoryManagementSystem.ApiTests.Helper;
using SmartInventoryManagementSystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using SmartInventoryManagementSystem.Application.DTO.SaleDTO;
using FluentAssertions;

namespace SmartInventoryManagementSystem.ApiTests.SalesEndpoints
{
    public class SalesControllerTests
    {

        [Fact]
        public async Task CreateSale_ShouldReturnOk_WhenServiceSucceeds()
        {
            await using var factory = new ApiFactory();

            var mockSaleService = new Mock<ISaleService>();
            mockSaleService.Setup(s => s.CreateSaleAsync(It.IsAny<CreateSaleRequest>()))
                           .ReturnsAsync(new CreateSaleResponse
                           {
                               SaleId = 123,
                               TotalAmount = 75m,
                               ItemCount = 2
                           });

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ISaleService>();
                    services.AddScoped(_ => mockSaleService.Object);

                    // Mock IPdfService to avoid issues with PDF generation during tests
                    services.RemoveAll<IPdfService>();
                    services.AddScoped<IPdfService>(_ => Mock.Of<IPdfService>());

                });
            }).CreateClient();

            var dto = new CreateSaleRequest
            {
                BuyerName = "Samin",
                Items =
        {
            new CreateSaleItemRequest { ProductId = 1, Quantity = 2 },
            new CreateSaleItemRequest { ProductId = 2, Quantity = 1 }
        }
            };

            var response = await client.PostAsJsonAsync("/api/sales/CreateSale", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CreateSaleResponse>();
            result!.SaleId.Should().Be(123);
            result.TotalAmount.Should().Be(75m);
            result.ItemCount.Should().Be(2);
        }


        [Fact]
        public async Task CreateSale_ShouldReturnBadRequest_WhenServiceThrows()
        {
            await using var factory = new ApiFactory();

            var mockSaleService = new Mock<ISaleService>();
            mockSaleService.Setup(s => s.CreateSaleAsync(It.IsAny<CreateSaleRequest>()))
                           .ThrowsAsync(new Exception("Something went wrong"));

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ISaleService>();
                    services.AddScoped(_ => mockSaleService.Object);

                    // Mock IPdfService to avoid issues with PDF generation during tests
                    services.RemoveAll<IPdfService>();
                    services.AddScoped<IPdfService>(_ => Mock.Of<IPdfService>());
                });
            }).CreateClient();

            var dto = new CreateSaleRequest
            {
                BuyerName = "Kaiser",
                Items =
        {
            new CreateSaleItemRequest { ProductId = 1, Quantity = 2 }
        }
            };

            var response = await client.PostAsJsonAsync("/api/sales/CreateSale", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Something went wrong");


        }


        [Fact]
        public async Task CreateSale_ShouldReturnUnauthorized_WhenNotAuthenticated()
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

            var dto = new CreateSaleRequest
            {
                BuyerName = "Nehal",
                Items =
        {
            new CreateSaleItemRequest { ProductId = 1, Quantity = 2 }
        }
            };

            var response = await client.PostAsJsonAsync("/api/sales/CreateSale", dto);

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
