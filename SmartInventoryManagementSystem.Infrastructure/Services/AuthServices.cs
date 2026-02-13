using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartInventoryManagementSystem.Application.DTO.AuthDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthServices(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<User?> RegisterUserAsync(AuthUserInfo ruserinfo)
        {
            if (ruserinfo == null || string.IsNullOrWhiteSpace(ruserinfo.Password))
            {
                return null;
            }

            if (await _context.Users.AnyAsync(u => u.Email == ruserinfo.Email || u.Username == ruserinfo.UserName))
            {
                return null; // User with the same email or username already exists
            }

            var user = new User
            {
                Username = ruserinfo.UserName,
                Email = ruserinfo.Email
            };

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, ruserinfo.Password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<TokenResponse?> LoginUserAsync(AuthUserInfo luserInfoReq)
        {
            var identifier = luserInfoReq.UserName?.Trim(); // the input from login form

            var userInstance = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);

            if (userInstance == null)
            {
                return null; // User not found
            }

            // Ensure PasswordHash and provided password are not null
            if (string.IsNullOrEmpty(userInstance.PasswordHash) || string.IsNullOrEmpty(luserInfoReq.Password))
            {
                return null; // Invalid password or password hash
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(userInstance, userInstance.PasswordHash, luserInfoReq.Password) == PasswordVerificationResult.Failed)
            {
                return null; // Invalid password
            }
            var GenToken = new TokenResponse
            {
                AccessToken = GenerateToken(userInstance),
                RefreshToken = await GenerateRefreshToken(userInstance)
            };
            return GenToken;
        }

        private async Task <string> GenerateRefreshToken(User RTuser)
        {
            var randomNumber = new byte[32];
            using var RNG = RandomNumberGenerator.Create();
            RNG.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);
            RTuser.RefreshToken = refreshToken;
            RTuser.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        private string GenerateToken(User userInfoTokenReq)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfoTokenReq.UserId.ToString()),
                new Claim (ClaimTypes.Name, userInfoTokenReq.Username??string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, userInfoTokenReq.UserId.ToString())
            };

            var TokenBuilder = new JwtSecurityToken
                (
                    issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                    audience: _configuration.GetValue<string>("AppSettings:Audience"),
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(TokenBuilder);
        }

        public async Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest rtuserInfoReq)
        {
            var userInstance = await _context.Users.FindAsync(rtuserInfoReq.UserId);
            if (userInstance == null || userInstance.RefreshToken != rtuserInfoReq.RefreshToken
                || userInstance.RefreshTokenExpiryDate <= DateTime.UtcNow)
            {
                return null; // Invalid refresh token or user not found
            }
            var GenToken = new TokenResponse
            {
                AccessToken = GenerateToken(userInstance),
                RefreshToken = await GenerateRefreshToken(userInstance)
            };
            return GenToken;
        }
    }
}
