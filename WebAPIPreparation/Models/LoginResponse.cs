namespace WebAPIPreparation.Models
{
     

        public class LoginResponse
        {
            public string UserName { get; set; } = "";
            public string Role { get; set; } = "";
            public string? AccessToken { get; set; }
            public string? RefreshToken { get; set; }
            public DateTime ExpiresAt { get; set; }
            public string[] Permissions { get; set; } = [];
        }

      
    

}
