namespace RMitra.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "HOMELY";
    public string Audience { get; set; } = "HOMELY.Clients";
    public string Secret { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 120;
}

public class OtpOptions
{
    public const string SectionName = "Otp";
    public int Length { get; set; } = 6;
    public int ExpiryMinutes { get; set; } = 5;
    public int MaxSendPer15Minutes { get; set; } = 3;
    public int MaxVerificationAttempts { get; set; } = 5;
    public bool ReturnOtpInResponse { get; set; }
}

public class CommerceOptions
{
    public const string SectionName = "Commerce";
    public decimal DeliveryFee { get; set; } = 30;
    public decimal HomelyPlusFreeDeliveryMin { get; set; } = 199;
    public decimal HomelyPlusItemDiscountPercent { get; set; } = 5;
    public decimal CommissionPercent { get; set; } = 15;
    public decimal GstPercent { get; set; } = 18;
    public decimal RiderSharePercent { get; set; } = 80;
    public decimal ExtraAfter3Km { get; set; } = 5;
    public decimal ExtraAfter6Km { get; set; } = 10;
    public int KitchenAcceptMinutes { get; set; } = 10;
    public int RiderOfferSeconds { get; set; } = 45;
    public int RiderSearchKm { get; set; } = 4;
    public int MaxRiderOffers { get; set; } = 3;
    public int KitchenSelfFallbackMinutes { get; set; } = 5;
}
