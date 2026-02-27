using FluentAssertions;
using Moq;
using SmartInventoryManagementSystem.ApiTests.Helper;
using SmartInventoryManagementSystem.Application.DTO.AuthDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SmartInventoryManagementSystem.ApiTests.AuthEndpoints
{
    public class AuthEndpointTests : IClassFixture<ApiFactory>
    {

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
        {
            await using var factory = new ApiFactory();

            var mockAuth = new Mock<IAuthServices>();
            mockAuth.Setup(x => x.RegisterUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync("some-user-id");

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthServices>();
                    services.AddScoped(_ => mockAuth.Object);
                });
            }).CreateClient();

            var dto = new RegisterUser
            {
                UserName = "user",
                Email = "email@test.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var response = await client.PostAsJsonAsync("/api/auth/register", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenModelStateInvalid()
        {
            await using var factory = new ApiFactory();
            var client = factory.CreateClient();

            var dto = new RegisterUser
            {
                UserName = null, // invalid
                Email = "email@test.com",
                Password = "pass"
            };

            var response = await client.PostAsJsonAsync("/api/auth/register", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenServiceReturnsNull()
        {
            await using var factory = new ApiFactory();

            var mockAuth = new Mock<IAuthServices>();
            mockAuth.Setup(x => x.RegisterUserAsync("user", "email@test.com", "pass"))
                    .ReturnsAsync((string?)null);

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthServices>();
                    services.AddScoped(_ => mockAuth.Object);
                });
            }).CreateClient();

            var dto = new RegisterUser
            {
                UserName = "user",
                Email = "email@test.com",
                Password = "pass"
            };

            var response = await client.PostAsJsonAsync("/api/auth/register", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        {
            await using var factory = new ApiFactory();

            var mockAuth = new Mock<IAuthServices>();
            mockAuth.Setup(x => x.LoginUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new TokenResponse
                    {
                        AccessToken = "access-token",
                        RefreshToken = "refresh-token"
                    });

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthServices>();
                    services.AddScoped(_ => mockAuth.Object);
                });
            }).CreateClient();

            var dto = new LoginUser
            {
                Identifier = "user@example.com",
                Password = "Password123!"
            };

            var response = await client.PostAsJsonAsync("/api/auth/login", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            token!.AccessToken.Should().Be("access-token");
        }


        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsInvalid()
        {
            await using var factory = new ApiFactory();

            var mockAuth = new Mock<IAuthServices>();
            mockAuth.Setup(x => x.LoginUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync((TokenResponse?)null);

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthServices>();
                    services.AddScoped(_ => mockAuth.Object);
                });
            }).CreateClient();

            var dto = new LoginUser
            {
                Identifier = "wrong@example.com",
                Password = "WrongPassword!"
            };

            var response = await client.PostAsJsonAsync("/api/auth/login", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenModelStateInvalid()
        {
            await using var factory = new ApiFactory();
            var client = factory.CreateClient();

            var dto = new LoginUser
            {
                Identifier = null,   // invalid
                Password = "Password123!"
            };

            var response = await client.PostAsJsonAsync("/api/auth/login", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact]
        public async Task Refresh_ShouldReturnOk_WhenRefreshTokenValid()
        {
            await using var factory = new ApiFactory();

            var mockAuth = new Mock<IAuthServices>();
            mockAuth.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenRequest>()))
                    .ReturnsAsync(new TokenResponse
                    {
                        AccessToken = "new-access-token",
                        RefreshToken = "new-refresh-token"
                    });

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthServices>();
                    services.AddScoped(_ => mockAuth.Object);
                });
            }).CreateClient();

            var dto = new RefreshTokenRequest
            {
                RefreshToken = "valid-refresh-token"
            };

            var response = await client.PostAsJsonAsync("/api/auth/refresh", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            token!.AccessToken.Should().Be("new-access-token");
        }


        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshTokenInvalid()
        {
            await using var factory = new ApiFactory();

            var mockAuth = new Mock<IAuthServices>();
            mockAuth.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenRequest>()))
                    .ReturnsAsync((TokenResponse?)null);

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthServices>();
                    services.AddScoped(_ => mockAuth.Object);
                });
            }).CreateClient();

            var dto = new RefreshTokenRequest
            {
                RefreshToken = "invalid-refresh-token"
            };

            var response = await client.PostAsJsonAsync("/api/auth/refresh", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task Refresh_ShouldReturnBadRequest_WhenModelStateInvalid()
        {
            await using var factory = new ApiFactory();
            var client = factory.CreateClient();

            var dto = new RefreshTokenRequest
            {
                RefreshToken = null // invalid
            };

            var response = await client.PostAsJsonAsync("/api/auth/refresh", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


    }

}
