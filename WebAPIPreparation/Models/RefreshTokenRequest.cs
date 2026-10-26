using System.ComponentModel.DataAnnotations;

namespace WebAPIPreparation.Models
{


    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = "";
    }
}
