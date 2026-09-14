using RM.DataModel.MobileVerification;
using RM.DataRepository.MobileVerification;
using RM.Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static RM.Infrastructure.CommonClass.StatusMessage;

namespace RM_Backend_API.Controllers
{
    [ApiController]
    [Route("api/rasoi-mitra/v1/mobile")]
    public class MobileVerificationController : ControllerBase
    {
        private readonly IOtpRepository _otpRepository;

        public MobileVerificationController(IOtpRepository otpRepository)
        {
            _otpRepository = otpRepository;
        }

        /// <summary>
        /// Sends a 6-digit OTP to the provided mobile number. OTP expires in 5 minutes. Max 3 resend attempts.
        /// </summary>
        [HttpPost("send-otp")]
        public async Task<ActionResult<ApiResponseMessage>> SendOtp([FromBody] SendOtpRequest request)
        {
            var result = await _otpRepository.SendOtpAsync(request);

            return Ok(new ApiResponseMessage
            {
                Success = true,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Success,
                Message = "OTP sent successfully.",
                Method_Name = nameof(SendOtp),
                Data = result
            });
        }

        /// <summary>
        /// Verifies the OTP and returns a verification token required for kitchen registration.
        /// </summary>
        [HttpPost("verify-otp")]
        public ActionResult<ApiResponseMessage> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var result = _otpRepository.VerifyOtp(request);

            return Ok(new ApiResponseMessage
            {
                Success = true,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Success,
                Message = "Mobile number verified successfully.",
                Method_Name = nameof(VerifyOtp),
                Data = result
            });
        }
    }
}
