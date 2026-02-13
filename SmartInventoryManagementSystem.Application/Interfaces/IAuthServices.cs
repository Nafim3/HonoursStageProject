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
        Task<TokenResponse?> LoginUserAsync(AuthUserInfo luserInfoReq);
        Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest rtuserInfoReq);
        Task<User?> RegisterUserAsync(AuthUserInfo ruserinfo);
    }
}
