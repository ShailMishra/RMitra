using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RM.Infrastructure.CommonClass;
using System.Net.Http.Headers;
using System.Text;

namespace RM.DataRepository.Sms
{
    public class HttpSmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpSmsService> _logger;
        private readonly SmsSettings _smsSettings;

        public HttpSmsService(HttpClient httpClient, ILogger<HttpSmsService> logger, SmsSettings smsSettings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _smsSettings = smsSettings;
        }

        public async Task SendOtpAsync(string mobileNumber, string otp, int expiryMinutes)
        {
            if (string.IsNullOrWhiteSpace(_smsSettings.ApiUrl))
            {
                _logger.LogWarning("SMS ApiUrl is not configured. OTP was not sent to {MobileNumber}.", mobileNumber);
                return;
            }

            var message = _smsSettings.OtpMessageTemplate
                .Replace("{otp}", otp, StringComparison.Ordinal)
                .Replace("{expiryMinutes}", expiryMinutes.ToString(), StringComparison.Ordinal);

            var payload = new
            {
                sender = _smsSettings.SenderId,
                template_id = _smsSettings.TemplateId,
                mobile = mobileNumber,
                otp,
                message,
                expiryMinutes
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _smsSettings.ApiUrl);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            if (!string.IsNullOrWhiteSpace(_smsSettings.ApiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _smsSettings.ApiKey);
                request.Headers.TryAddWithoutValidation("api_key", _smsSettings.ApiKey);
            }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "SMS gateway returned {StatusCode} for {MobileNumber}. Response: {Body}",
                    (int)response.StatusCode,
                    mobileNumber,
                    body);

                throw new SmsDeliveryException($"Failed to send OTP SMS. Gateway returned {(int)response.StatusCode}.");
            }

            _logger.LogInformation("OTP SMS dispatched to {MobileNumber} via HTTP provider.", mobileNumber);
        }
    }

    public class SmsDeliveryException : Exception
    {
        public SmsDeliveryException(string message) : base(message) { }
    }
}
