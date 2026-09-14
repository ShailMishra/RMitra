namespace RMitra.Domain.Customer;

public class CustomerAddress
{
    public Guid Id { get; set; }
    public string AddressId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Label { get; set; } = "Home";
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
