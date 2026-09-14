namespace RMitra.BuildingBlocks.Security;

public static class Roles
{
    public const string Customer = "CUSTOMER";
    public const string KitchenOwner = "KITCHEN_OWNER";
    public const string Rider = "RIDER";
    public const string Admin = "ADMIN";

    public static readonly string[] All = [Customer, KitchenOwner, Rider, Admin];

    public static bool IsValid(string? role) =>
        !string.IsNullOrWhiteSpace(role) && All.Contains(role, StringComparer.OrdinalIgnoreCase);
}

public static class AuthPurposes
{
    public const string Customer = "CUSTOMER";
    public const string KitchenOwner = "KITCHEN_OWNER";
    public const string Rider = "RIDER";

    public static readonly string[] All = [Customer, KitchenOwner, Rider];

    public static bool IsValid(string? purpose) =>
        !string.IsNullOrWhiteSpace(purpose) && All.Contains(purpose, StringComparer.OrdinalIgnoreCase);
}
