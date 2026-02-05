using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO
{
    public class RefreshTokenRequest
    {
        public string? RefreshToken { get; set; }
        public int UserId { get; set; }
    }
}
