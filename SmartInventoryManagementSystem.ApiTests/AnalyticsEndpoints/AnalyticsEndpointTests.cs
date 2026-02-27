using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInventoryManagementSystem.ApiTests.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.ApiTests.AnalyticsEndpoints
{
    public class AnalyticsEndpointTests
    {

        [Fact]
        public async Task GetSalesAnalysis_ShouldReturnOk()
        {
            await using var factory = new ApiFactory();
            var client = factory.CreateClient();

            var response = await client.GetAsync("/api/analytics/chart");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Fact]
        public async Task GetSalesAnalysis_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            await using var factory = new ApiFactory();

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // This removes the fake auth handler
                    services.RemoveAll<IAuthenticationSchemeProvider>();
                    services.RemoveAll<IAuthenticationHandlerProvider>();

                    // "None" scheme that always fails
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

            var response = await client.GetAsync("/api/analytics/chart");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private class NoAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public NoAuthHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder)
                : base(options, logger, encoder)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                return Task.FromResult(AuthenticateResult.Fail("No authentication"));
            }
        }


    }
}
