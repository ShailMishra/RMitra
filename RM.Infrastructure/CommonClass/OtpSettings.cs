namespace RM.Infrastructure.CommonClass
{
    public class OtpSettings
    {
        public int Length { get; set; } = 6;
        public int ExpiryMinutes { get; set; } = 5;
        public int MaxResendAttempts { get; set; } = 3;
        public int MaxVerificationAttempts { get; set; } = 5;
        public int VerificationTokenExpiryMinutes { get; set; } = 30;
    }
}
