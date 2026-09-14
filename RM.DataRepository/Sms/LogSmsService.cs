using Microsoft.Extensions.Logging;
using RM.Infrastructure.CommonClass;

namespace RM.DataRepository.Sms
{
    public class LogSmsService : ISmsService
    {
        private readonly ILogger<LogSmsService> _logger;
        private readonly SmsSettings _smsSettings;

        public LogSmsService(ILogger<LogSmsService> logger, SmsSettings smsSettings)
        {
            _logger = logger;
            _smsSettings = smsSettings;
        }

        public Task SendOtpAsync(string mobileNumber, string otp, int expiryMinutes)
        {
            var message = _smsSettings.OtpMessageTemplate
                .Replace("{otp}", otp, StringComparison.Ordinal)
                .Replace("{expiryMinutes}", expiryMinutes.ToString(), StringComparison.Ordinal);

            _logger.LogInformation(
                "[SMS-Log] OTP sent to {MobileNumber}: {Message}",
                mobileNumber,
                message);

            return Task.CompletedTask;
        }
    }
}
