namespace RMitra.Domain.Common;

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Blocked = 3
}

public static class KitchenStatuses
{
    public const string Draft = "DRAFT";
    public const string MobileVerified = "MOBILE_VERIFIED";
    public const string Submitted = "SUBMITTED";
    public const string DocumentVerificationPending = "DOCUMENT_VERIFICATION_PENDING";
    public const string UnderReview = "UNDER_REVIEW";
    public const string AdditionalDocumentsRequired = "ADDITIONAL_DOCUMENTS_REQUIRED";
    public const string Resubmitted = "RESUBMITTED";
    public const string Rejected = "REJECTED";
    public const string Approved = "APPROVED";
    public const string MenuSetup = "MENU_SETUP";
    public const string Active = "ACTIVE";
}

public static class OrderStatuses
{
    public const string PaymentPending = "PAYMENT_PENDING";
    public const string PaymentFailed = "PAYMENT_FAILED";
    public const string Placed = "PLACED";
    public const string Accepted = "ACCEPTED";
    public const string Preparing = "PREPARING";
    public const string Ready = "READY";
    public const string OutForDelivery = "OUT_FOR_DELIVERY";
    public const string Delivered = "DELIVERED";
    public const string Cancelled = "CANCELLED";
    public const string Rejected = "REJECTED";
    public const string Refunded = "REFUNDED";
}

public static class PaymentStatuses
{
    public const string Pending = "PENDING";
    public const string Success = "SUCCESS";
    public const string Failed = "FAILED";
    public const string Refunded = "REFUNDED";
}

public static class PaymentMethods
{
    public const string Upi = "UPI";
    public const string Card = "CARD";
    public const string Netbanking = "NETBANKING";
    public const string Wallet = "WALLET";
    public const string Cod = "COD";

    public static readonly string[] All = [Upi, Card, Netbanking, Wallet, Cod];
}

public static class DeliveryModes
{
    public const string Rider = "RIDER";
    public const string KitchenSelf = "KITCHEN_SELF";
}

public static class RiderStatuses
{
    public const string Draft = "DRAFT";
    public const string Submitted = "SUBMITTED";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
}

public static class AssignmentStatuses
{
    public const string Offered = "OFFERED";
    public const string Accepted = "ACCEPTED";
    public const string Declined = "DECLINED";
    public const string Expired = "EXPIRED";
}

public static class TripStatuses
{
    public const string Assigned = "ASSIGNED";
    public const string ArrivedKitchen = "ARRIVED_KITCHEN";
    public const string PickedUp = "PICKED_UP";
    public const string Delivered = "DELIVERED";
}

public static class SettlementStatuses
{
    public const string Pending = "PENDING";
    public const string Paid = "PAID";
    public const string Failed = "FAILED";
}

public static class DocumentTypes
{
    public const string KitchenPhoto = "KITCHEN_PHOTO";
    public const string PanCard = "PAN_CARD";
    public const string FssaiLicense = "FSSAI_LICENSE";
    public const string Aadhaar = "AADHAAR";
    public const string DrivingLicence = "DRIVING_LICENCE";
    public const string VehicleRc = "VEHICLE_RC";
    public const string Selfie = "SELFIE";
}
