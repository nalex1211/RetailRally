namespace RetailRally.Helpers;

public static class Roles
{
    public static class Role
    {
        public const string Admin = "admin";
        public const string Vendor = "vendor";
        public const string Customer = "customer";
        public static List<string> AllRoles => new List<string> { Admin, Vendor, Customer };
    }
}
