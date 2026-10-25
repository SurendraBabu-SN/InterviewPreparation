namespace WebAPIPreparation.Auth
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";

        public static readonly string[] All = [Admin, Manager, User];

        public static bool IsValid(string? role) =>
        role is not null && All.Contains(role, StringComparer.OrdinalIgnoreCase);

        /// <summary>Returns the role with correct casing, e.g. "admin" → "Admin".</summary>
        public static string Normalize(string role) =>
            All.First(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}
