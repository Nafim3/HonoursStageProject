using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO.AuthDTO;
using SmartInventoryManagementSystem.Application.DTO.DeleteUserDTO;
using SmartInventoryManagementSystem.Application.Interfaces;

namespace SmartInventoryManagementSystem.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authService;
        private readonly ICurrentUserService _currentUser;

        public AuthController(IAuthServices authService, ICurrentUserService currentUser)
        {
            _authService = authService;
            _currentUser = currentUser;
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
            var user = await _authService.LoginUserAsync(dto.Identifier!, dto.Password!);
            if (user == null)
                return Unauthorized();
            return Ok(user);
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

        [Authorize]
        [HttpPost("delete-account/me")]

        public async Task<IActionResult> DeleteUser([FromBody] DeleteAccountRequest request)
        {
            var currentUserId = _currentUser.UserId;

            if (currentUserId == 0)
                return Unauthorized();

            var result = await _authService.SoftDeleteUserAsync(currentUserId, request.Password);

            if (result == "WrongPassword")
                return BadRequest("Incorrect password");

            if (result == "UserNotFound")
                return NotFound();

            return Ok("Account deleted successfully");
        }

    }
}
