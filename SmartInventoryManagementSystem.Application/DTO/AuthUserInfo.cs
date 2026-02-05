using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO
{
    public class AuthUserInfo
    {
        [Required]
        public string? UserName { get; set;}
       
        public string? Email { get; set; }        
        [Required]
        public string? Password { get; set; }
    }
}
