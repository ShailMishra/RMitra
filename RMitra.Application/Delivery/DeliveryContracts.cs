using System.ComponentModel.DataAnnotations;
using RMitra.Application.Identity;

namespace RMitra.Application.Delivery;

public class RegisterRiderRequest
{
    [Required] public string VerificationToken { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public string VehicleType { get; set; } = "BIKE";
    public BankDetails? BankDetails { get; set; }
}

public class BankDetails
{
    public string Holder { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string Ifsc { get; set; } = string.Empty;
}

public class RegisterRiderResponse
{
    public bool Success { get; set; } = true;
    public string RiderId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserProfileDto User { get; set; } = new();
}

public class PresenceRequest
{
    public bool Available { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

public class AssignmentDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? DropAddress { get; set; }
}

public class RespondAssignmentRequest
{
    [Required] public string Decision { get; set; } = "ACCEPT";
}

public class TrackingRequest
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

public class RiderReviewRequest
{
    [Required] public string Decision { get; set; } = "APPROVE";
    public string? Reason { get; set; }
}

public interface IRiderService
{
    Task<RegisterRiderResponse> RegisterAsync(RegisterRiderRequest request, CancellationToken cancellationToken = default);
    Task AddDocumentAsync(Guid userId, string documentType, string fileUrl, CancellationToken cancellationToken = default);
    Task SubmitAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SetPresenceAsync(Guid userId, PresenceRequest request, CancellationToken cancellationToken = default);
    Task<AssignmentDto?> GetCurrentAssignmentAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AssignmentDto> RespondAsync(Guid userId, string assignmentId, RespondAssignmentRequest request, CancellationToken cancellationToken = default);
    Task UpdateTripStatusAsync(Guid userId, string orderId, string status, CancellationToken cancellationToken = default);
    Task UpdateTrackingAsync(string orderId, Guid userId, TrackingRequest request, CancellationToken cancellationToken = default);
    Task ReviewAsync(string riderId, RiderReviewRequest request, CancellationToken cancellationToken = default);
    Task StartAssignmentForReadyOrderAsync(Guid orderGuid, CancellationToken cancellationToken = default);
}
