namespace SafeVault.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public enum UserRole
    {
        User = 0,
        Admin = 1
    }
}
