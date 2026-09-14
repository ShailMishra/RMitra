namespace RMitra.BuildingBlocks.Security;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string Role { get; }
    string Mobile { get; }
    bool IsInRole(string role);
}
