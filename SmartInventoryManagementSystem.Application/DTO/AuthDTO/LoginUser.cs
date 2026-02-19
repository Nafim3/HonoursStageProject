using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO.AuthDTO
{
    public class LoginUser
    {
        [Required(ErrorMessage = " Either Username or Email is required")]
        public string? Identifier { get; set;}
          
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
