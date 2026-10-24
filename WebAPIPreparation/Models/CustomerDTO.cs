using System.ComponentModel.DataAnnotations;

namespace WebAPIPreparation.Models
{
    public record CustomerResponse(int Id, string CustomerName, string? City, string? State, string? Country);

    public record CreateCustomerRequest(
        [Range(1, int.MaxValue)] int Id,
        [Required, StringLength(100)] string CustomerName,
        [StringLength(50)] string? City,
        [StringLength(100)] string? State,
        [StringLength(100)] string? Country);

    public record UpdateCustomerRequest(
        [Required, StringLength(100)] string CustomerName,
        [StringLength(50)] string? City,
        [StringLength(100)] string? State,
        [StringLength(100)] string? Country);
}
