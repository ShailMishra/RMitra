namespace RMitra.Domain.Kitchen;

public class Kitchen
{
    public Guid Id { get; set; }
    public string KitchenId { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string KitchenType { get; set; } = "HOMEMADE";
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? CuisineTypes { get; set; }
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
    public int DeliveryRadiusKm { get; set; } = 10;
    public string? AccountHolderName { get; set; }
    public string? AccountNumber { get; set; }
    public string? IfscCode { get; set; }
    public string? AccountType { get; set; }
    public string Status { get; set; } = "DRAFT";
    public bool ListingEnabled { get; set; }
    public string? RejectionReason { get; set; }
    public string? AdditionalDocumentsRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
}

public class KitchenDocument
{
    public Guid Id { get; set; }
    public Guid KitchenGuid { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
