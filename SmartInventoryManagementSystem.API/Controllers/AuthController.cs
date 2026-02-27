using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO.AuthDTO;
using SmartInventoryManagementSystem.Application.Interfaces;

namespace SmartInventoryManagementSystem.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authService;

        public AuthController(IAuthServices authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterUser dto)
        {
            var result = await _authService.RegisterUserAsync(dto.UserName!, dto.Email!, dto.Password!);

            if (result == "Email already exists" || result == "Username already exists")
                return BadRequest(new { message = result });

            return Ok(new { message = "Registration successful" });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUser dto)
        {
            var Token = await _authService.LoginUserAsync(dto.Identifier!, dto.Password!);
            if (Token == null)
                return Unauthorized();

            return Ok(Token);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var TokenGen = await _authService.RefreshTokenAsync(dto);
            if (TokenGen == null)
                return Unauthorized();

            return Ok(TokenGen);
        }

    }
}
