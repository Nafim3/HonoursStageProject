using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SmartInventoryManagementSystem.API.Controllers;
using SmartInventoryManagementSystem.ApiTests.Helper;
using SmartInventoryManagementSystem.Application.DTO.ProductDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.ApiTests.ProductEndpoints
{
    public class ProductControllerTests
    {
        [Fact]
        public async Task GetProducts_ShouldReturnOk_WithMappedDto()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetProductAsync())
                    .ReturnsAsync(new List<Product>
                    {
                    new Product
                    {
                        ProductId = 1,
                        ProductName = "Milk",
                        QuantityInStock = 10,
                        ReorderLevel = 3,
                        ProductPrice = 5
                    }
                    });

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.AddScoped(_ => mockRepo.Object);
                });
            }).CreateClient();

            var response = await client.GetAsync("/api/product");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<IEnumerable<ProductListDto>>();

            dto.Should().ContainSingle();

            var item = dto!.First();
            item.ProductId.Should().Be(1);
            item.Name.Should().Be("Milk");
            item.Quantity.Should().Be(10);
            item.ReorderLevel.Should().Be(3);
            item.Price.Should().Be(5);
        }


        [Fact]
        public async Task GetProductById_ShouldReturnOk_WhenProductExistsAndBelongsToUser()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            var mockUser = new Mock<ICurrentUserService>();

            mockUser.Setup(u => u.UserId).Returns(1);

            mockRepo.Setup(r => r.GetProductByIdAsync(1))
                    .ReturnsAsync(new Product
                    {
                        ProductId = 1,
                        ProductName = "Milk",
                        QuantityInStock = 10,
                        ReorderLevel = 3,
                        ProductPrice = 5,
                        UserId = 1
                    });

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ICurrentUserService>();

                    services.AddScoped(_ => mockRepo.Object);
                    services.AddScoped(_ => mockUser.Object);
                });
            }).CreateClient();

            var response = await client.GetAsync("/api/product/1");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var product = await response.Content.ReadFromJsonAsync<Product>();
            product!.ProductId.Should().Be(1);
            product.ProductName.Should().Be("Milk");
        }

        [Fact]
        public async Task AddProduct_ShouldReturnOk_WhenProductIsAdded()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            var mockUser = new Mock<ICurrentUserService>();

            mockUser.Setup(u => u.UserId).Returns(1);

            mockRepo.Setup(r => r.AddProductAsync(It.IsAny<Product>()))
                    .ReturnsAsync((string?)null); // success

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ICurrentUserService>();

                    services.AddScoped(_ => mockRepo.Object);
                    services.AddScoped(_ => mockUser.Object);
                });
            }).CreateClient();

            var dto = new CreateProductRequest
            {
                Name = "Milk",
                Quantity = 10,
                ReorderLevel = 3,
                Price = 5,
                ExpiryDate = DateTime.UtcNow.AddDays(10)
            };

            var response = await client.PostAsJsonAsync("/api/product/Addproducts", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AddProduct_ShouldReturnBadRequest_WhenRepositoryReturnsError()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            var mockUser = new Mock<ICurrentUserService>();

            mockUser.Setup(u => u.UserId).Returns(1);

            mockRepo.Setup(r => r.AddProductAsync(It.IsAny<Product>()))
                    .ReturnsAsync("Duplicate product name");

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ICurrentUserService>();

                    services.AddScoped(_ => mockRepo.Object);
                    services.AddScoped(_ => mockUser.Object);
                });
            }).CreateClient();

            var dto = new CreateProductRequest
            {
                Name = "Milk",
                Quantity = 10,
                ReorderLevel = 3,
                Price = 5,
                ExpiryDate = DateTime.UtcNow.AddDays(10)
            };

            var response = await client.PostAsJsonAsync("/api/product/Addproducts", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Duplicate product name");
        }


        [Fact]
        public async Task UpdateProduct_ShouldReturnNoContent_WhenUpdateSucceeds()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            var mockUser = new Mock<ICurrentUserService>();

            mockUser.Setup(u => u.UserId).Returns(1);

            mockRepo.Setup(r => r.UpdateProductAsync(It.IsAny<Product>()))
                    .ReturnsAsync((string?)null); // success

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ICurrentUserService>();

                    services.AddScoped(_ => mockRepo.Object);
                    services.AddScoped(_ => mockUser.Object);
                });
            }).CreateClient();

            var dto = new UpdateProductRequest
            {
                Name = "Milk",
                Quantity = 10,
                Reorder_Level = 3,
                Price = 5,
                ExpiryDate = DateTime.UtcNow.AddDays(10)
            };

            var response = await client.PutAsJsonAsync("/api/product/UpdateProduct/1", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        [Fact]
        public async Task UpdateProduct_ShouldReturnValidationProblem_WhenRepositoryReturnsError()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            var mockUser = new Mock<ICurrentUserService>();

            mockUser.Setup(u => u.UserId).Returns(1);

            mockRepo.Setup(r => r.UpdateProductAsync(It.IsAny<Product>()))
                    .ReturnsAsync("Expiry date cannot be in the past");

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ICurrentUserService>();

                    services.AddScoped(_ => mockRepo.Object);
                    services.AddScoped(_ => mockUser.Object);
                });
            }).CreateClient();

            var dto = new UpdateProductRequest
            {
                Name = "Milk",
                Quantity = 10,
                Reorder_Level = 3,
                Price = 5,
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            };

            var response = await client.PutAsJsonAsync("/api/product/UpdateProduct/1", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("ExpiryDate");
            body.Should().Contain("Expiry date cannot be in the past");
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnValidationProblem_WhenModelStateInvalid()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            var mockUser = new Mock<ICurrentUserService>();

            mockUser.Setup(u => u.UserId).Returns(1);

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ICurrentUserService>();

                    services.AddScoped(_ => mockRepo.Object);
                    services.AddScoped(_ => mockUser.Object);
                });
            }).CreateClient();

            var dto = new UpdateProductRequest(); // invalid

            var response = await client.PutAsJsonAsync("/api/product/UpdateProduct/1", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("errors");
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnNoContent_WhenProductExistsAndBelongsToUser()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            var mockUser = new Mock<ICurrentUserService>();

            mockUser.Setup(u => u.UserId).Returns(1);

            mockRepo.Setup(r => r.GetProductByIdAsync(1))
                    .ReturnsAsync(new Product
                    {
                        ProductId = 1,
                        UserId = 1
                    });

            mockRepo.Setup(r => r.DeleteProductAsync(1))
                .ReturnsAsync((string?)null);


            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ICurrentUserService>();

                    services.AddScoped(_ => mockRepo.Object);
                    services.AddScoped(_ => mockUser.Object);
                });
            }).CreateClient();

            var response = await client.DeleteAsync("/api/product/DeleteProduct/1");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteProductController_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            var mockUser = new Mock<ICurrentUserService>();

            mockUser.Setup(u => u.UserId).Returns(1);

            mockRepo.Setup(r => r.GetProductByIdAsync(1))
                    .ReturnsAsync((Product?)null);

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ICurrentUserService>();

                    services.AddScoped(_ => mockRepo.Object);
                    services.AddScoped(_ => mockUser.Object);
                });
            }).CreateClient();

            var response = await client.DeleteAsync("/api/product/DeleteProduct/1");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Product with ID 1 not found.");
        }

        [Fact]
        public async Task CheckExpiredProducts_ShouldReturnOk_AndNotifyForEachExpiredProduct()
        {
            await using var factory = new ApiFactory();

            var mockRepo = new Mock<IProductRepository>();
            var mockUser = new Mock<ICurrentUserService>();
            var mockNotification = new Mock<INotificationService>();

            mockUser.Setup(u => u.UserId).Returns(1);

            mockRepo.Setup(r => r.GetExpiredProductsAsync(1))
                    .ReturnsAsync(new List<Product>
                    {
                new Product { ProductId = 1, ProductName = "Milk", UserId = 1 },
                new Product { ProductId = 2, ProductName = "Eggs", UserId = 1 }
                    });

            mockNotification.Setup(n => n.NotifyExpiredProductsAsync(It.IsAny<Product>(), 1))
                            .Returns(Task.CompletedTask);

            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ICurrentUserService>();
                    services.RemoveAll<INotificationService>();

                    services.AddScoped(_ => mockRepo.Object);
                    services.AddScoped(_ => mockUser.Object);
                    services.AddScoped(_ => mockNotification.Object);
                });
            }).CreateClient();

            var response = await client.GetAsync("/api/product/Expired-products/check");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            mockRepo.Verify(r => r.GetExpiredProductsAsync(1), Times.Once);
            mockNotification.Verify(n => n.NotifyExpiredProductsAsync(It.IsAny<Product>(), 1), Times.Exactly(2));
        }

        [Fact]
        public async Task CheckExpiredProducts_ShouldReturnUnauthorized_WhenNotAuthenticated()
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

            var response = await client.GetAsync("/api/product/Expired-products/check");

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













