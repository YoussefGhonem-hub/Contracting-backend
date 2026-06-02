namespace Contracting.Shared.Constants
{
    public static class RoleNames
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Teamleadengineer = "Teamlead-engineer";
        public const string Siteengineer = "Site-engineer";
        public const string Officeengineer = "Office-engineer";
        public const string Accounts = "Accounts";
        public const string IT = "IT";
        public const string Viewer = "Viewer";
        public const string Client = "Client";

        /// <summary>
        /// Gets all available role names
        /// </summary>
        public static readonly string[] All = new[]
        {
            SuperAdmin,
            Admin,
            Teamleadengineer,
            Siteengineer,
            Officeengineer,
            Accounts,
            IT,
            Viewer,
            Client
        };

        /// <summary>
        /// Checks if a role name is valid
        /// </summary>
        public static bool IsValid(string roleName)
        {
            return All.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
