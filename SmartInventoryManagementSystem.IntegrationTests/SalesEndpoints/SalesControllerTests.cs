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
