using Microsoft.AspNetCore.Mvc;
using WebAPIPreparation.Models;
using WebAPIPreparation.Services;

namespace WebAPIPreparation.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            // Database validation example

            if (request.UserName != "admin" ||
                request.Password != "Admin@123")
            {
                return Unauthorized();
            }

            var accessToken =
                _jwtService.GenerateAccessToken(
                    request.UserName,
                    "Admin");

            var refreshToken =
                _jwtService.GenerateRefreshToken();

            // Save Refresh Token in DB

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken(
            RefreshTokenRequest request)
        {
            // Get user by Refresh Token from DB

            bool validRefreshToken = true;

            if (!validRefreshToken)
            {
                return Unauthorized();
            }

            var newAccessToken =
                _jwtService.GenerateAccessToken(
                    "admin",
                    "Admin");

            var newRefreshToken =
                _jwtService.GenerateRefreshToken();

            return Ok(new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            });
        }
    }
}
