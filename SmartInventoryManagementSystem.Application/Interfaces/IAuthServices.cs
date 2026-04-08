using SmartInventoryManagementSystem.Application.DTO.AuthDTO;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.Interfaces
{
    public interface IAuthServices
    {
        Task<TokenResponse?> LoginUserAsync(string identifier, string password);
        Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest rtuserInfoReq);
        Task<string?> RegisterUserAsync(string username, string email, string password);
        Task<string> SoftDeleteUserAsync(int userId, string password);
    }
}
