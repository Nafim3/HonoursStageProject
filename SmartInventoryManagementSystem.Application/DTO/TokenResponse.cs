using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO
{
    public class TokenResponse
    {
        public string? RefreshToken { get; set; }
        public string? AccessToken { get; set; }
    }
}
