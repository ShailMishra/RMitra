using System.Security.Claims;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.BuildingBlocks.Security;

namespace RMitra.Api.Security;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User
        ?? throw AppException.Unauthorized();

    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

    public Guid UserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id)
            ? id
            : throw AppException.Unauthorized();

    public string Role => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public string Mobile => User.FindFirstValue(ClaimTypes.MobilePhone) ?? string.Empty;

    public bool IsInRole(string role) => User.IsInRole(role);
}
