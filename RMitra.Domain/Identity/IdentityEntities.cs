using RMitra.Domain.Common;

namespace RMitra.Domain.Identity;

public class User
{
    public Guid Id { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OtpRequest
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string OtpHash { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
