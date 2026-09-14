namespace RM.Infrastructure.CommonClass
{
    public class SmsSettings
    {
        public bool Enabled { get; set; } = true;

        /// <summary>Log (development) or Http (production SMS gateway).</summary>
        public string Provider { get; set; } = "Log";

        public string ApiUrl { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;

        public string SenderId { get; set; } = "RASOIM";

        public string TemplateId { get; set; } = string.Empty;

        public string OtpMessageTemplate { get; set; } =
            "Your RasoiMitra OTP is {otp}. Valid for {expiryMinutes} minutes. Do not share with anyone.";
    }
}
