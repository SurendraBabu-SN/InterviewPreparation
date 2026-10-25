
using WebAPIPreparation.Auth;
using global::WebAPIPreparation.Models;
using global::WebAPIPreparation.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace WebAPIPreparation.Services
{


   

    public interface IUserService
    {
        Task<UserResponse> SignUpAsync(SignUpRequest request, CancellationToken ct);
        Task<UserResponse> CreateByAdminAsync(AdminCreateUserRequest request, CancellationToken ct);
        Task<List<UserResponse>> GetAllAsync(CancellationToken ct);
        Task<UserResponse> GetByIdAsync(int id, CancellationToken ct);
        Task<UserResponse> ChangeRoleAsync(int id, string role, CancellationToken ct);
        Task<UserResponse> ChangeStatusAsync(int id, bool isActive, CancellationToken ct);
        Task ChangePasswordAsync(int id, ChangePasswordRequest request, CancellationToken ct);
    }

    public class UserService(AppDbContext db, IPasswordHasher<User> passwordHasher) : IUserService
    {
        // ---------- Sign-up by the user themselves ----------
        public async Task<UserResponse> SignUpAsync(SignUpRequest request, CancellationToken ct)
        {
            // The very first account becomes Admin; everyone after that is a normal User.
            var isFirstUser = !await db.Users.AnyAsync(ct);
            var role = isFirstUser ? Roles.Admin : Roles.User;

            return await CreateUserAsync(request.UserName, request.Password, request.Email, role, ct);
        }

        // ---------- User created by an Admin (any role) ----------
        public async Task<UserResponse> CreateByAdminAsync(AdminCreateUserRequest request, CancellationToken ct)
        {
            if (!Roles.IsValid(request.Role))
                throw new BadRequestException($"Role '{request.Role}' does not exist. Use: {string.Join(", ", Roles.All)}.");

            return await CreateUserAsync(request.UserName, request.Password, request.Email,
                                         Roles.Normalize(request.Role), ct);
        }

        // ---------- Shared create logic ----------
        private async Task<UserResponse> CreateUserAsync(
            string userName, string password, string? email, string role, CancellationToken ct)
        {
            userName = userName.Trim();

            if (await db.Users.AnyAsync(u => u.UserName == userName, ct))
                throw new ConflictException($"User name '{userName}' is already taken.");

            var user = new User
            {
                UserName = userName,
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                Role = role,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            // Never store the real password – only a salted hash (PBKDF2).
            user.PasswordHash = passwordHasher.HashPassword(user, password);

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);   // Id is IDENTITY, SQL Server fills it
            return ToResponse(user);
            
        }

        // ---------- Admin: read ----------
        public Task<List<UserResponse>> GetAllAsync(CancellationToken ct) =>
            db.Users.AsNoTracking()
                .OrderBy(u => u.UserName)
                .Select(u => new UserResponse(u.Id, u.UserName, u.Email, u.Role,
                                              u.IsActive, u.CreatedDate, u.ModifiedDate))
                .ToListAsync(ct);

        public async Task<UserResponse> GetByIdAsync(int id, CancellationToken ct)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct)
                       ?? throw new NotFoundException($"User {id} not found.");
            return ToResponse(user);
        }

        // ---------- Admin: change role ----------
        public async Task<UserResponse> ChangeRoleAsync(int id, string role, CancellationToken ct)
        {
            if (!Roles.IsValid(role))
                throw new BadRequestException($"Role '{role}' does not exist.");

            var user = await FindAsync(id, ct);
            var newRole = Roles.Normalize(role);

            // Do not allow removing the last Admin – otherwise nobody can manage users.
            if (user.Role == Roles.Admin && newRole != Roles.Admin && await IsLastActiveAdmin(user.Id, ct))
                throw new BadRequestException("You cannot remove the last Admin.");

            user.Role = newRole;
            SignOut(user);                    // new role applies at next login
            await db.SaveChangesAsync(ct);
            return ToResponse(user);
        }

        // ---------- Admin: activate / deactivate ----------
        public async Task<UserResponse> ChangeStatusAsync(int id, bool isActive, CancellationToken ct)
        {
            var user = await FindAsync(id, ct);

            if (!isActive && user.Role == Roles.Admin && await IsLastActiveAdmin(user.Id, ct))
                throw new BadRequestException("You cannot deactivate the last Admin.");

            user.IsActive = isActive;
            if (!isActive) SignOut(user);     // deactivated users are logged out
            else user.ModifiedDate = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
            return ToResponse(user);
        }

        // ---------- Any user: change own password ----------
        public async Task ChangePasswordAsync(int id, ChangePasswordRequest request, CancellationToken ct)
        {
            var user = await FindAsync(id, ct);

            var check = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (check == PasswordVerificationResult.Failed)
                throw new BadRequestException("Current password is not correct.");

            user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
            SignOut(user);                    // log out other devices
            await db.SaveChangesAsync(ct);
        }

        // ---------- Helpers ----------
        private async Task<User> FindAsync(int id, CancellationToken ct) =>
            await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException($"User {id} not found.");

        private Task<bool> IsLastActiveAdmin(int userId, CancellationToken ct) =>
            db.Users.AllAsync(u => u.Id == userId || u.Role != Roles.Admin || !u.IsActive, ct);

        private static void SignOut(User user)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.ModifiedDate = DateTime.UtcNow;
        }

        private static UserResponse ToResponse(User u) =>
            new(u.Id, u.UserName, u.Email, u.Role, u.IsActive, u.CreatedDate, u.ModifiedDate);
    }
}
