using System.Security.Cryptography;
using System.Text;

namespace RMitra.Infrastructure.Security;

internal static class OtpHasher
{
    public static string Hash(string otp, string mobileNumber)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{otp}|{mobileNumber}|RMitra"));
        return Convert.ToHexString(bytes);
    }
}
