namespace RMitra.BuildingBlocks.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public IReadOnlyList<object> Details { get; }

    public AppException(int statusCode, string errorCode, string message, IReadOnlyList<object>? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details ?? [];
    }

    public static AppException Validation(string message, IReadOnlyList<object>? details = null) =>
        new(400, "VALIDATION_ERROR", message, details);

    public static AppException Unauthorized(string message = "Missing token or invalid OTP.") =>
        new(401, "UNAUTHORIZED", message);

    public static AppException Forbidden(string message = "You are not allowed to perform this action.", string code = "FORBIDDEN") =>
        new(403, code, message);

    public static AppException ListingNotEnabled() =>
        new(403, "LISTING_NOT_ENABLED", "Menu APIs unlock only after kitchen approval.");

    public static AppException NotFound(string message, string code = "NOT_FOUND") =>
        new(404, code, message);

    public static AppException Conflict(string message, string code = "CONFLICT") =>
        new(409, code, message);

    public static AppException Unprocessable(string message, string code) =>
        new(422, code, message);

    public static AppException RateLimited(string message = "OTP throttled.") =>
        new(429, "RATE_LIMITED", message);
}
