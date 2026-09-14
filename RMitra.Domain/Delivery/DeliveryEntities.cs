namespace RMitra.Domain.Delivery;

public class Rider
{
    public Guid Id { get; set; }
    public string RiderId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = "BIKE";
    public string Status { get; set; } = "DRAFT";
    public bool Available { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? AccountHolderName { get; set; }
    public string? AccountNumber { get; set; }
    public string? IfscCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RiderDocument
{
    public Guid Id { get; set; }
    public Guid RiderGuid { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RiderAssignment
{
    public Guid Id { get; set; }
    public string AssignmentId { get; set; } = string.Empty;
    public Guid OrderGuid { get; set; }
    public Guid RiderGuid { get; set; }
    public string Status { get; set; } = "OFFERED";
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DeliveryJob
{
    public Guid Id { get; set; }
    public Guid OrderGuid { get; set; }
    public Guid? RiderGuid { get; set; }
    public string DeliveryMode { get; set; } = "RIDER";
    public string Status { get; set; } = "ASSIGNED";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
