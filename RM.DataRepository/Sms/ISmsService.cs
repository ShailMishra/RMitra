namespace RM.DataRepository.Sms
{
    public interface ISmsService
    {
        Task SendOtpAsync(string mobileNumber, string otp, int expiryMinutes);
    }
}
