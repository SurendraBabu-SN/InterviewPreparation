using System.ComponentModel.DataAnnotations;

namespace WebAPIPreparation.Models
{
    public class LoginRequest
    {
        [Required]  public  string UserName { get; set; } = "";
        [Required]  public   string Password { get; set; } = "";

    }

    /// <summary>Self sign-up: no Role field, so nobody can make themselves Admin.</summary>
    public record SignUpRequest(
        [Required, StringLength(100, MinimumLength = 3)] string UserName,
        [Required, StringLength(100, MinimumLength = 8)] string Password,
        [EmailAddress, StringLength(200)] string? Email);

    /// <summary>Admin creates a user and chooses the role.</summary>
    public record AdminCreateUserRequest(
        [Required, StringLength(100, MinimumLength = 3)] string UserName,
        [Required, StringLength(100, MinimumLength = 8)] string Password,
        [EmailAddress, StringLength(200)] string? Email,
        [Required, StringLength(50)] string Role);

    public record ChangeRoleRequest([Required, StringLength(50)] string Role);

    public record ChangeStatusRequest(bool IsActive);

    public record ChangePasswordRequest(
        [Required] string CurrentPassword,
        [Required, StringLength(100, MinimumLength = 8)] string NewPassword);

    /// <summary>What the API returns. PasswordHash and RefreshToken are never sent out.</summary>
    public record UserResponse(
        int Id, string UserName, string? Email, string Role,
        bool IsActive, DateTime CreatedDate, DateTime? ModifiedDate);

}
