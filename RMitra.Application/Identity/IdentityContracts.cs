using System.ComponentModel.DataAnnotations;
using RMitra.Domain.Identity;

namespace RMitra.Application.Identity;

public class SendOtpRequest
{
    [Required, RegularExpression(@"^[6-9]\d{9}$")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    public string Purpose { get; set; } = string.Empty;

    public string? Method_Name { get; set; }
}

public class SendOtpResponse
{
    public bool Success { get; set; } = true;
    public string RequestId { get; set; } = string.Empty;
    public string? DebugOtp { get; set; }
}

public class VerifyOtpRequest
{
    [Required]
    public string RequestId { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[6-9]\d{9}$")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{6}$")]
    public string Otp { get; set; } = string.Empty;

    public string? Method_Name { get; set; }
}

public class VerifyOtpResponse
{
    public bool Success { get; set; } = true;
    public bool IsNewUser { get; set; }
    public string? VerificationToken { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserProfileDto? User { get; set; }
}

public class SetPasswordRequest
{
    [Required]
    public string VerificationToken { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, RegularExpression(@"^[6-9]\d{9}$")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    public string Purpose { get; set; } = string.Empty;

    public string? Password { get; set; }
    public string? RequestId { get; set; }
    public string? Otp { get; set; }
}

public class AuthSessionResponse
{
    public bool Success { get; set; } = true;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserProfileDto User { get; set; } = new();
}

public class UserProfileDto
{
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;

    public static UserProfileDto From(User user) => new()
    {
        UserCode = user.UserCode,
        FullName = user.FullName,
        MobileNumber = user.MobileNumber,
        Email = user.Email,
        Role = user.Role
    };
}

public interface IIdentityService
{
    Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request, CancellationToken cancellationToken = default);
    Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
    Task SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<AuthSessionResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
