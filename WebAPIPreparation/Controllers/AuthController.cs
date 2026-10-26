using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIPreparation.Models;
using WebAPIPreparation.Services;

namespace WebAPIPreparation.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
       
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
        {
            var response = await authService.LoginAsync(request, ct);
            if (response is null)
                return Unauthorized(new { message = "Wrong user name or password." });

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request, CancellationToken ct)
        {
            var response = await authService.RefreshAsync(request.RefreshToken, ct);
            if (response is null)
                return Unauthorized(new { message = "Refresh token is invalid or expired. Please log in again." });

            return Ok(response);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct)
        {
            await authService.LogoutAsync(request.RefreshToken, ct);
            return NoContent();
        }

    }
}
