using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAPIPreparation.Models;


namespace WebAPIPreparation.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken ct);
    Task LogoutAsync(string refreshToken, CancellationToken ct);

    // ✅ GenerateAccessToken / GenerateRefreshToken removed from the interface.
    //    The controller must not be able to create tokens without checks.

    //string GenerateAccessToken(string? userName, string role);
    //string GenerateRefreshToken();
}

public class AuthService(
    AppDbContext db,
    IPasswordHasher<User> passwordHasher,
    IConfiguration config) : IAuthService
{
    // ✅ ONE setting name for the access token life, used everywhere
    private int AccessTokenMinutes => config.GetValue("Jwt:AccessTokenMinutes", 30);
    private int RefreshTokenDays => config.GetValue("Jwt:RefreshTokenDays", 7);

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrEmpty(request.Password))
            return null;

        // 1. Find the user in the Users table
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.UserName == request.UserName.Trim(), ct);

        if (user is null || !user.IsActive)
            return null;

        // 2. Compare the typed password with the stored hash
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        // Hash was made with older settings → upgrade it silently
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        // 3. Create tokens and save the refresh token
        return await IssueTokensAsync(user, ct);
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var hash = Hash(refreshToken);
        var user = await db.Users.FirstOrDefaultAsync(u => u.RefreshToken == hash, ct);

        if (user is null || !user.IsActive ||
            user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return null;

        return await IssueTokensAsync(user, ct);   // new refresh token; the old one stops working
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;

        var hash = Hash(refreshToken);
        await db.Users.Where(u => u.RefreshToken == hash)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.RefreshToken, (string?)null)
                .SetProperty(u => u.RefreshTokenExpiryTime, (DateTime?)null), ct);
    }

    private async Task<LoginResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        // Role comes from the database, not hard-coded
        var expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenMinutes);   // ✅ same value for token and response
        var accessToken = GenerateAccessToken(user, expiresAt);
        var refreshToken = GenerateRefreshToken();

        // Save only the HASH of the refresh token
        user.RefreshToken = Hash(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenDays);
        user.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new LoginResponse
        {
            UserName = user.UserName,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = refreshToken,      // the real value goes only to the client
            ExpiresAt = DateTime.UtcNow.AddMinutes(AccessTokenMinutes)
        };
    }




    private string GenerateAccessToken(User user, DateTime expiresAt)
    {

        // ✅ clear error if the key is missing or too short
        var keyText = config["Jwt:Key"];
        if (string.IsNullOrEmpty(keyText) || Encoding.UTF8.GetByteCount(keyText) < 32)
            throw new InvalidOperationException("Jwt:Key is missing or shorter than 32 characters in appsettings.json.");


        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),   // ✅ user Id, handy in controllers
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


        var token = new JwtSecurityToken(
                  issuer: config["Jwt:Issuer"],
                  audience: config["Jwt:Audience"],
                  claims: claims,
                  expires: expiresAt,      // ✅ no more DurationInMinutes
                  signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];

        using var rng = RandomNumberGenerator.Create();

        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private static string Hash(string value) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}