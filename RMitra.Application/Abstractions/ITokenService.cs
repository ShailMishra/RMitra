using RMitra.Domain.Identity;

namespace RMitra.Application.Abstractions;

public interface ITokenService
{
    string CreateAccessToken(User user);
    DateTime GetExpiryUtc();
}
