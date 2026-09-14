using RM.DataModel.MobileVerification;
using RM.DataRepository.Sms;
using RM.Infrastructure.CommonClass;
using Microsoft.Extensions.Caching.Memory;

namespace RM.DataRepository.MobileVerification
{
    public interface IOtpRepository
    {
        Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request);
        VerifyOtpResponse VerifyOtp(VerifyOtpRequest request);
        bool IsMobileVerified(string verificationToken, string mobileNumber);
    }

    public class OtpRepository : IOtpRepository
    {
        private const string OtpSessionCachePrefix = "otp_session_";
        private const string VerificationTokenCachePrefix = "mob_ver_";

        private readonly IMemoryCache _cache;
        private readonly OtpSettings _otpSettings;
        private readonly ISmsService _smsService;
        private readonly SmsSettings _smsSettings;
        private readonly Random _random = new();

        public OtpRepository(
            IMemoryCache cache,
            OtpSettings otpSettings,
            ISmsService smsService,
            SmsSettings smsSettings)
        {
            _cache = cache;
            _otpSettings = otpSettings;
            _smsService = smsService;
            _smsSettings = smsSettings;
        }

        public async Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request)
        {
            var mobileNumber = request.MobileNumber.Trim();
            var existingSessionKey = GetActiveSessionKeyForMobile(mobileNumber);

            if (existingSessionKey != null && _cache.TryGetValue(existingSessionKey, out OtpSession? existingSession) && existingSession != null)
            {
                if (existingSession.ResendCount >= _otpSettings.MaxResendAttempts)
                {
                    throw new OtpException(StatusMessage.StatusInformation.Max_Resend_Attempts_Exceeded,
                        $"Maximum resend attempts ({_otpSettings.MaxResendAttempts}) exceeded. Please try again later.");
                }

                existingSession.ResendCount++;
                existingSession.Otp = GenerateOtp();
                existingSession.ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes);
                existingSession.VerificationAttempts = 0;

                _cache.Set(existingSessionKey, existingSession, existingSession.ExpiresAt);

                await DispatchOtpAsync(mobileNumber, existingSession.Otp);

                return new SendOtpResponse
                {
                    Success = true,
                    RequestId = existingSession.RequestId
                };
            }

            var requestId = $"OTP_REQ_{Random.Shared.Next(100000, 999999)}";
            var sessionKey = $"{OtpSessionCachePrefix}{requestId}";
            var expiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes);

            var session = new OtpSession
            {
                RequestId = requestId,
                MobileNumber = mobileNumber,
                Otp = GenerateOtp(),
                ExpiresAt = expiresAt,
                ResendCount = 0,
                VerificationAttempts = 0,
                IsVerified = false
            };

            _cache.Set(sessionKey, session, expiresAt);
            _cache.Set(GetMobileSessionLookupKey(mobileNumber), sessionKey, expiresAt);

            await DispatchOtpAsync(mobileNumber, session.Otp);

            return new SendOtpResponse
            {
                Success = true,
                RequestId = requestId
            };
        }

        public VerifyOtpResponse VerifyOtp(VerifyOtpRequest request)
        {
            var sessionKey = $"{OtpSessionCachePrefix}{request.RequestId}";
            if (!_cache.TryGetValue(sessionKey, out OtpSession? session) || session == null)
            {
                throw new OtpException(StatusMessage.StatusInformation.Otp_Expired, "OTP request not found or has expired.");
            }

            if (!string.Equals(session.MobileNumber, request.MobileNumber.Trim(), StringComparison.Ordinal))
            {
                throw new OtpException(StatusMessage.StatusInformation.Invalid_Otp, "Invalid OTP or mobile number.");
            }

            if (DateTime.UtcNow > session.ExpiresAt)
            {
                _cache.Remove(sessionKey);
                throw new OtpException(StatusMessage.StatusInformation.Otp_Expired, "OTP has expired. Please request a new OTP.");
            }

            if (session.VerificationAttempts >= _otpSettings.MaxVerificationAttempts)
            {
                throw new OtpException(StatusMessage.StatusInformation.Otp_Max_Attempts_Exceeded,
                    $"Maximum verification attempts ({_otpSettings.MaxVerificationAttempts}) exceeded.");
            }

            session.VerificationAttempts++;

            if (!string.Equals(session.Otp, request.Otp.Trim(), StringComparison.Ordinal))
            {
                _cache.Set(sessionKey, session, session.ExpiresAt);
                throw new OtpException(StatusMessage.StatusInformation.Invalid_Otp, "Invalid OTP. Please try again.");
            }

            var verificationToken = $"mob_ver_{Guid.NewGuid():N}";
            session.IsVerified = true;
            session.VerificationToken = verificationToken;
            _cache.Set(sessionKey, session, session.ExpiresAt);

            var tokenExpiry = DateTime.UtcNow.AddMinutes(_otpSettings.VerificationTokenExpiryMinutes);
            _cache.Set($"{VerificationTokenCachePrefix}{verificationToken}", new MobileVerificationToken
            {
                MobileNumber = session.MobileNumber,
                RequestId = session.RequestId,
                ExpiresAt = tokenExpiry
            }, tokenExpiry);

            return new VerifyOtpResponse
            {
                Success = true,
                VerificationToken = verificationToken
            };
        }

        public bool IsMobileVerified(string verificationToken, string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(verificationToken))
                return false;

            var cacheKey = $"{VerificationTokenCachePrefix}{verificationToken.Trim()}";
            if (!_cache.TryGetValue(cacheKey, out MobileVerificationToken? token) || token == null)
                return false;

            if (DateTime.UtcNow > token.ExpiresAt)
            {
                _cache.Remove(cacheKey);
                return false;
            }

            return string.Equals(token.MobileNumber, mobileNumber.Trim(), StringComparison.Ordinal);
        }

        private async Task DispatchOtpAsync(string mobileNumber, string otp)
        {
            if (!_smsSettings.Enabled)
                return;

            await _smsService.SendOtpAsync(mobileNumber, otp, _otpSettings.ExpiryMinutes);
        }

        private string GenerateOtp()
        {
            var min = (int)Math.Pow(10, _otpSettings.Length - 1);
            var max = (int)Math.Pow(10, _otpSettings.Length) - 1;
            return _random.Next(min, max + 1).ToString();
        }

        private static string GetMobileSessionLookupKey(string mobileNumber) => $"otp_mobile_lookup_{mobileNumber}";

        private string? GetActiveSessionKeyForMobile(string mobileNumber)
        {
            var lookupKey = GetMobileSessionLookupKey(mobileNumber);
            return _cache.TryGetValue(lookupKey, out string? sessionKey) ? sessionKey : null;
        }

        private sealed class OtpSession
        {
            public string RequestId { get; set; } = string.Empty;
            public string MobileNumber { get; set; } = string.Empty;
            public string Otp { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public int ResendCount { get; set; }
            public int VerificationAttempts { get; set; }
            public bool IsVerified { get; set; }
            public string? VerificationToken { get; set; }
        }

        private sealed class MobileVerificationToken
        {
            public string MobileNumber { get; set; } = string.Empty;
            public string RequestId { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
        }
    }

    public class OtpException : Exception
    {
        public StatusMessage.StatusInformation StatusCode { get; }

        public OtpException(StatusMessage.StatusInformation statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
