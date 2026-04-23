using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using SmartInventoryManagementSystem.Application.DTO.AuthDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.UnitTests.InfrastructureTests.ServicesTests
{
    public class AuthTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterUserAsync_ReturnsError_WhenEmailExists()
        {
            var context = GetInMemoryDbContext();
            var configMock = new Mock<IConfiguration>();
            var service = new AuthServices(configMock.Object, context);

            context.Users.Add(new User
            {
                Username = "existingUser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });

            await context.SaveChangesAsync();

            var result = await service.RegisterUserAsync("newUser", "test@example.com", "password123");

            result.Should().Be("Email already exists");
        }

        [Fact]
        public async Task RegisterUserAsync_ReturnsError_WhenUsernameExists()
        {
            var context = GetInMemoryDbContext();
            var configMock = new Mock<IConfiguration>();
            var service = new AuthServices(configMock.Object, context);

            context.Users.Add(new User
            {
                Username = "existingUser",
                Email = "unique@example.com",
                PasswordHash = "hash"
            });

            await context.SaveChangesAsync();

            var result = await service.RegisterUserAsync("existingUser", "new@example.com", "password123");

            result.Should().Be("Username already exists");
        }

        [Fact]
        public async Task RegisterUserAsync_CreatesUser_WhenDataIsValid()
        {
            var context = GetInMemoryDbContext();
            var configMock = new Mock<IConfiguration>();
            var service = new AuthServices(configMock.Object, context);

            var result = await service.RegisterUserAsync("newUser", "new@example.com", "password123");

            result.Should().NotBeNull(); 
            int.Parse(result!).Should().BeGreaterThan(0);

            var createdUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "new@example.com");

            createdUser.Should().NotBeNull();
            createdUser!.Username.Should().Be("newUser");
            createdUser.PasswordHash.Should().NotBeNullOrEmpty();
            createdUser.PasswordHash.Should().NotBe("password123");

        }


        [Fact]
        public async Task LoginUserAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            var configMock = new Mock<IConfiguration>();
            var service = new AuthServices(configMock.Object, context);

            var result = await service.LoginUserAsync("unknown@example.com", "password123");

            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginUserAsync_ReturnsNull_WhenPasswordIsIncorrect()
        {
            var context = GetInMemoryDbContext();
            var configMock = new Mock<IConfiguration>();
            var service = new AuthServices(configMock.Object, context);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com"
            };

            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, "correctPassword");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await service.LoginUserAsync("test@example.com", "wrongPassword");

            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginUserAsync_ReturnsTokenResponse_WhenCredentialsAreValid()
        {
            var context = GetInMemoryDbContext();

            var inMemorySettings = new Dictionary<string, string?>
{
                { "AppSettings:Token", "supersecretkey123456789083tyghbuvvbh8yghgnvvlkgjijlvjio0oghoighqeg" },
                { "AppSettings:Issuer", "TestIssuer" },
                { "AppSettings:Audience", "TestAudience" }
};

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new AuthServices(config, context);


            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com"
            };

            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, "correctPassword");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await service.LoginUserAsync("test@example.com", "correctPassword");

            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

        }

        // actual test for GenerateRefreshToken method,
        // which is private, so its triggered through LoginUserAsync and
        // then it checks the database for the expected changes
        [Fact]
        public async Task GenerateRefreshToken_SetsTokenAndExpiry_AndPersistsToDatabase()
        {
            var context = GetInMemoryDbContext();

            var inMemorySettings = new Dictionary<string, string?>
    {
                    { "AppSettings:Token", "supersecretkey1234567890jio4574tytyfgwgffgrfnfhuhhghegi93trw7r9wir[r23" },
                    { "AppSettings:Issuer", "TestIssuer" },
                    { "AppSettings:Audience", "TestAudience" }
    };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new AuthServices(config, context);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com"
            };

            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, "correctPassword");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // login triggers GenerateRefreshToken internally
            var result = await service.LoginUserAsync("test@example.com", "correctPassword");

           
            result.Should().NotBeNull();
            result!.RefreshToken.Should().NotBeNullOrEmpty();

            var updatedUser = await context.Users.FirstAsync();

            updatedUser.RefreshToken.Should().Be(result.RefreshToken);
            updatedUser.RefreshTokenExpiryDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
        }

        // This is also a private method,
        // so its tested indirectly through LoginUserAsync
        // and then validates the structure and claims of the generated JWT token
        [Fact]
        public async Task LoginUserAsync_GeneratesValidJwtToken_WithCorrectClaims()
        {
            
            var context = GetInMemoryDbContext();

            var inMemorySettings = new Dictionary<string, string?>
    {
                { "AppSettings:Token", "supersecretkey1234567890ddueuh2486/*g43873g`#/./vsje830ger9546eqgg35ti9yyjyt4t34tk" },
                { "AppSettings:Issuer", "TestIssuer" },
                { "AppSettings:Audience", "TestAudience" }
    };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new AuthServices(config, context);

            var user = new User
            {
                UserId = 42,
                Username = "testuser",
                Email = "test@example.com"
            };

            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, "correctPassword");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            
            var result = await service.LoginUserAsync("test@example.com", "correctPassword");

            
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();

            // Validate JWT structure
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.AccessToken);

            token.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "42");
            token.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
            token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "42");

            token.Issuer.Should().Be("TestIssuer");
            token.Audiences.Should().Contain("TestAudience");

            token.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            var context = GetInMemoryDbContext();

            var inMemorySettings = new Dictionary<string, string?>
    {
        { "AppSettings:Token", "supersecretkey1234567890usghe724378tbsjbvdd9w8whfbsbvvdf541iffwjbfffrwf48y49" },
        { "AppSettings:Issuer", "TestIssuer" },
        { "AppSettings:Audience", "TestAudience" }
    };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new AuthServices(config, context);

            var request = new RefreshTokenRequest
            {
                UserId = 478,
                RefreshToken = "anything"
            };

            var result = await service.RefreshTokenAsync(request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNull_WhenRefreshTokenDoesNotMatch()
        {
            var context = GetInMemoryDbContext();

            var inMemorySettings = new Dictionary<string, string?>
    {
        { "AppSettings:Token", "supersecretkey1234567890usghe724378tbsjbvdd9w8whfbsbvvdf541iffwjbfffrwf48y49" },
        { "AppSettings:Issuer", "TestIssuer" },
        { "AppSettings:Audience", "TestAudience" }
    };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new AuthServices(config, context);

            var user = new User
            {
                UserId = 1,
                Username = "testuser",
                RefreshToken = "valid-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            user.PasswordHash = new PasswordHasher<User>()
            .HashPassword(user, "somePassword");


            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new RefreshTokenRequest
            {
                UserId = 1,
                RefreshToken = "wrong-token"
            };

            var result = await service.RefreshTokenAsync(request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNull_WhenRefreshTokenExpired()
        {
            var context = GetInMemoryDbContext();

            var inMemorySettings = new Dictionary<string, string?>
    {
        { "AppSettings:Token", "supersecretkey1234567890usghe724378tbsjbvdd9w8whfbsbvvdf541iffwjbfffrwf48y49" },
        { "AppSettings:Issuer", "TestIssuer" },
        { "AppSettings:Audience", "TestAudience" }
    };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new AuthServices(config, context);

            var user = new User
            {
                UserId = 1,
                Username = "testuser",
                RefreshToken = "valid-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(-1) // expired
            };

            user.PasswordHash = new PasswordHasher<User>()
            .HashPassword(user, "somePassword");


            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new RefreshTokenRequest
            {
                UserId = 1,
                RefreshToken = "valid-token"
            };

            var result = await service.RefreshTokenAsync(request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNewTokens_WhenRefreshTokenIsValid()
        {
            var context = GetInMemoryDbContext();

            var inMemorySettings = new Dictionary<string, string?>
    {
        { "AppSettings:Token", "supersecretkey1234567890usghe724378tbsjbvdd9w8whfbsbvvdf541iffwjbfffrwf48y49" },
        { "AppSettings:Issuer", "TestIssuer" },
        { "AppSettings:Audience", "TestAudience" }
    };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new AuthServices(config, context);

            var user = new User
            {
                UserId = 1,
                Username = "testuser",
                RefreshToken = "valid-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            user.PasswordHash = new PasswordHasher<User>()
            .HashPassword(user, "somePassword");


            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new RefreshTokenRequest
            {
                UserId = 1,
                RefreshToken = "valid-token"
            };

            var result = await service.RefreshTokenAsync(request);

            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

            var updatedUser = await context.Users.FindAsync(1);
            updatedUser!.RefreshToken.Should().Be(result.RefreshToken);
        }

        [Fact]
        public async Task SoftDeleteUserAsync_WhenUserNotFound_ReturnsUserNotFound()
        {

            var context = GetInMemoryDbContext();

            var mockConfig = new Mock<IConfiguration>();
            var service = new AuthServices(mockConfig.Object, context);


            var result = await service.SoftDeleteUserAsync(1, "password");


            Assert.Equal("UserNotFound", result);
        }


        [Fact]
        public async Task SoftDeleteUserAsync_WhenPasswordIsIncorrect_ReturnsWrongPassword()
        {

            var context = GetInMemoryDbContext();

            var user = new User
            {
                UserId = 1,
                PasswordHash = new PasswordHasher<User>().HashPassword(new User(), "correct")
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var mockConfig = new Mock<IConfiguration>();
            var service = new AuthServices(mockConfig.Object, context);

            var result = await service.SoftDeleteUserAsync(1, "wrong");


            Assert.Equal("WrongPassword", result);
        }


        [Fact]
        public async Task SoftDeleteUserAsync_WhenPasswordCorrect_DeletesUserAndReturnsDeleted()
        {

            var context = GetInMemoryDbContext();

            var user = new User
            {
                UserId = 1,
                PasswordHash = new PasswordHasher<User>().HashPassword(new User(), "correct"),
                IsDeleted = false
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var mockConfig = new Mock<IConfiguration>();
            var service = new AuthServices(mockConfig.Object, context);


            var result = await service.SoftDeleteUserAsync(1, "correct");


            Assert.Equal("Deleted", result);
            Assert.True(user.IsDeleted);
        }


    }
}
