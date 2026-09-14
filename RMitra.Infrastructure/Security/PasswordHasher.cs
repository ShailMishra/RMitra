using System.Security.Cryptography;
using System.Text.RegularExpressions;
using RMitra.Application.Abstractions;
using RMitra.BuildingBlocks.Exceptions;

namespace RMitra.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public void ValidateStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8 ||
            !Regex.IsMatch(password, "[A-Za-z]") || !Regex.IsMatch(password, "[0-9]"))
        {
            throw AppException.Validation("Password must be at least 8 characters and include a letter and a number.");
        }
    }

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var bytes = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(bytes)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split(':');
        if (parts.Length != 3 || parts[0] != "pbkdf2")
            return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
