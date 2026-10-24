using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebAPIPreparation.Models;
using WebAPIPreparation.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken ct);
    Task LogoutAsync(string refreshToken, CancellationToken ct);
}

public class AuthService(
    AppDbContext db,
    IJwtService jwtService,                  // your existing JWT service
    IPasswordHasher<User> passwordHasher,
    IConfiguration config) : IAuthService
{
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
        var hash = Hash(refreshToken);
        var user = await db.Users.FirstOrDefaultAsync(u => u.RefreshToken == hash, ct);

        if (user is null || !user.IsActive ||
            user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return null;

        return await IssueTokensAsync(user, ct);   // new refresh token; the old one stops working
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        await db.Users.Where(u => u.RefreshToken == hash)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.RefreshToken, (string?)null)
                .SetProperty(u => u.RefreshTokenExpiryTime, (DateTime?)null), ct);
    }

    private async Task<LoginResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        // Role comes from the database, not hard-coded
        var accessToken = jwtService.GenerateAccessToken(user.UserName, user.Role);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Save only the HASH of the refresh token
        user.RefreshToken = Hash(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenDays);
        user.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,      // the real value goes only to the client
            ExpiresAt = DateTime.UtcNow.AddMinutes(AccessTokenMinutes)
        };
    }

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}