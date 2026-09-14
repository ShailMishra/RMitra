using System.ComponentModel.DataAnnotations;

namespace RM.Infrastructure.CommonClass
{
    public class StatusMessage
    {
        public enum StatusInformation
        {
            [Display(Name = "Record Found")] Success = 1000,
            [Display(Name = "No Record Found")] Fail = 1001,
            [Display(Name = "API Key and Secret Key is null.Please Pass API Key and Secret Key")] API_Key_Secret_Key_Is_Null = 1002,
            [Display(Name = "API Key is Null.Please Pass API Key")] API_Key_Is_Null = 1003,
            [Display(Name = "Secret Key is Null.Please pass Secret Key")] Secret_Key_Is_Null = 1004,
            [Display(Name = "Please Try Again")] Internel_Error = 1005,
            [Display(Name = "API Key or Secret Key is Invalid")] API_Key_Is_Secret_Key_Invalid = 1006,
            [Display(Name = "Exception Code")] Exception_Code = 1009,
            [Display(Name = "Request JSON Body Is Null")] Request_JSON_Body_Is_Null = 1017,
            [Display(Name = "Manadatory Feild Required")] Manadatory_Feild_Required = 1025,
            [Display(Name = "Database Response")] Database_Response = 1026,
            [Display(Name = "OTP Expired")] Otp_Expired = 1027,
            [Display(Name = "OTP Max Attempts Exceeded")] Otp_Max_Attempts_Exceeded = 1028,
            [Display(Name = "Invalid OTP")] Invalid_Otp = 1029,
            [Display(Name = "Mobile Not Verified")] Mobile_Not_Verified = 1030,
            [Display(Name = "Invalid Verification Token")] Invalid_Verification_Token = 1031,
            [Display(Name = "Max Resend Attempts Exceeded")] Max_Resend_Attempts_Exceeded = 1032,
            [Display(Name = "Kitchen Not Found")] Kitchen_Not_Found = 1033,
            [Display(Name = "Invalid Kitchen Status Transition")] Invalid_Kitchen_Status_Transition = 1034
        }
    }
}
