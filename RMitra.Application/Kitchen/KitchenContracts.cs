using System.ComponentModel.DataAnnotations;

namespace RMitra.Application.Kitchen;

public class CreateKitchenRequest
{
    [Required] public string VerificationToken { get; set; } = string.Empty;
    [Required] public string KitchenName { get; set; } = string.Empty;
    [Required] public string OwnerName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string KitchenType { get; set; } = "HOMEMADE";
}

public class PatchKitchenRequest
{
    public string? KitchenName { get; set; }
    public string? OwnerName { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public List<string>? CuisineTypes { get; set; }
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
    public int? DeliveryRadiusKm { get; set; }
    public BankDetailsDto? BankDetails { get; set; }
}

public class BankDetailsDto
{
    public string Holder { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string Ifsc { get; set; } = string.Empty;
    public string Type { get; set; } = "SAVINGS";
}

public class KitchenDto
{
    public string KitchenId { get; set; } = string.Empty;
    public string KitchenName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string KitchenType { get; set; } = "HOMEMADE";
    public string Status { get; set; } = string.Empty;
    public bool ListingEnabled { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public List<string> CuisineTypes { get; set; } = [];
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
    public int DeliveryRadiusKm { get; set; }
    public string? RejectionReason { get; set; }
    public double? DistanceKm { get; set; }
}

public class KitchenDocumentDto
{
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public bool Required { get; set; }
}

public class KitchenDocumentsResponse
{
    public IReadOnlyList<KitchenDocumentDto> Documents { get; set; } = [];
    public IReadOnlyList<string> MissingRequired { get; set; } = [];
}

public class KitchenProgressDto
{
    public string KitchenId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public IReadOnlyList<ProgressStepDto> Checklist { get; set; } = [];
}

public class ProgressStepDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Done { get; set; }
}

public class KitchenDashboardDto
{
    public string KitchenId { get; set; } = string.Empty;
    public int TodayOrders { get; set; }
    public decimal TodayEarnings { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class NearbyKitchenDto
{
    public string KitchenId { get; set; } = string.Empty;
    public string KitchenName { get; set; } = string.Empty;
    public List<string> CuisineTypes { get; set; } = [];
    public double DistanceKm { get; set; }
    public bool OpenNow { get; set; }
    public bool Featured { get; set; }
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
}

public class RejectKitchenRequest
{
    [Required] public string Reason { get; set; } = string.Empty;
}

public class RequestDocumentsRequest
{
    [Required] public string DocumentsRequired { get; set; } = string.Empty;
}

public interface IKitchenService
{
    Task<KitchenDto> CreateAsync(Guid ownerUserId, CreateKitchenRequest request, CancellationToken cancellationToken = default);
    Task<KitchenDto> PatchAsync(string kitchenId, Guid ownerUserId, PatchKitchenRequest request, CancellationToken cancellationToken = default);
    Task<KitchenDto> GetAsync(string kitchenId, CancellationToken cancellationToken = default);
    Task<KitchenDocumentsResponse> AddDocumentAsync(string kitchenId, Guid ownerUserId, string documentType, string fileUrl, CancellationToken cancellationToken = default);
    Task<KitchenDocumentsResponse> GetDocumentsAsync(string kitchenId, CancellationToken cancellationToken = default);
    Task<KitchenDto> SubmitAsync(string kitchenId, Guid ownerUserId, string? idempotencyKey, CancellationToken cancellationToken = default);
    Task<KitchenProgressDto> GetProgressAsync(string kitchenId, CancellationToken cancellationToken = default);
    Task<KitchenDto> ActivateAsync(string kitchenId, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<KitchenDashboardDto> GetDashboardAsync(string kitchenId, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KitchenDto>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<KitchenDto> ApproveAsync(string kitchenId, CancellationToken cancellationToken = default);
    Task<KitchenDto> RejectAsync(string kitchenId, RejectKitchenRequest request, CancellationToken cancellationToken = default);
    Task<KitchenDto> RequestDocumentsAsync(string kitchenId, RequestDocumentsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NearbyKitchenDto>> NearbyAsync(decimal latitude, decimal longitude, bool openNow, CancellationToken cancellationToken = default);
}
