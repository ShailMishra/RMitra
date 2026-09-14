using System.ComponentModel.DataAnnotations;
using RMitra.Application.Identity;

namespace RMitra.Application.Customer;

public class RegisterCustomerRequest
{
    [Required] public string VerificationToken { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class RegisterCustomerResponse
{
    public bool Success { get; set; } = true;
    public string CustomerId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserProfileDto User { get; set; } = new();
}

public class AddressDto
{
    public string AddressId { get; set; } = string.Empty;
    public string Label { get; set; } = "Home";
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class UpsertAddressRequest
{
    [Required] public string Label { get; set; } = "Home";
    [Required] public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string State { get; set; } = string.Empty;
    [Required] public string Pincode { get; set; } = string.Empty;
    [Required] public decimal Latitude { get; set; }
    [Required] public decimal Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class PatchCustomerRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}

public interface ICustomerService
{
    Task<RegisterCustomerResponse> RegisterAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileDto> GetMeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto> PatchMeAsync(Guid userId, PatchCustomerRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AddressDto>> GetAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AddressDto> AddAddressAsync(Guid userId, UpsertAddressRequest request, CancellationToken cancellationToken = default);
}
