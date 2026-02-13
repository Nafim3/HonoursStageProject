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
        public async Task<IActionResult> Register(AuthUserInfo dto)
        {
            var userInstance = await _authService.RegisterUserAsync(dto);
            if (userInstance == null)
                return BadRequest("Username already exists");

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthUserInfo dto)
        {
            var Token = await _authService.LoginUserAsync(dto);
            if (Token == null)
                return Unauthorized();

            return Ok(Token);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest dto)
        {
            var TokenGen = await _authService.RefreshTokenAsync(dto);
            if (TokenGen == null)
                return Unauthorized();

            return Ok(TokenGen);
        }

    }
}
