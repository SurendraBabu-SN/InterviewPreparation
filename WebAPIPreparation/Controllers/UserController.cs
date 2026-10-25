using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPIPreparation.Auth;
using WebAPIPreparation.Models;
using WebAPIPreparation.Services;

namespace WebAPIPreparation.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(IUserService userService) : ControllerBase
{
    /// <summary>Anyone can create their own account. Gets the "User" role (first account gets "Admin").</summary>
    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp(SignUpRequest request, CancellationToken ct)
    {
        var user = await userService.SignUpAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Admin creates a user with any role.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(AdminCreateUserRequest request, CancellationToken ct)
    {
        var user = await userService.CreateByAdminAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await userService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        Ok(await userService.GetByIdAsync(id, ct));

    [HttpPut("{id:int}/role")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ChangeRole(int id, ChangeRoleRequest request, CancellationToken ct) =>
        Ok(await userService.ChangeRoleAsync(id, request.Role, ct));

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ChangeStatus(int id, ChangeStatusRequest request, CancellationToken ct) =>
        Ok(await userService.ChangeStatusAsync(id, request.IsActive, ct));

    /// <summary>A logged-in user changes their OWN password.</summary>
    [HttpPut("me/password")]
    [Authorize]
    public async Task<IActionResult> ChangeMyPassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var myId = await GetMyUserIdAsync();
        await userService.ChangePasswordAsync(myId, request, ct);
        return NoContent();
    }

    // Your JwtService puts the user name in the token (ClaimTypes.Name), not the Id,
    // so we look the Id up from the user list by name.
    private async Task<int> GetMyUserIdAsync()
    {
        var userName = User.FindFirstValue(ClaimTypes.Name);
        var all = await userService.GetAllAsync(HttpContext.RequestAborted);
        return all.First(u => u.UserName == userName).Id;
    }
}
