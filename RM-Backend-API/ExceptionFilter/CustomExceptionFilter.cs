using RM.DataRepository.Kitchen;
using RM.DataRepository.MobileVerification;
using RM.DataRepository.Sms;
using RM.Infrastructure.Response;
using RMitra.BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net;
using static RM.Infrastructure.CommonClass.StatusMessage;

namespace RM_Backend_API.ExceptionFilter
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var actionName = descriptor?.ActionName ?? string.Empty;
            var exception = context.Exception;

            if (exception is AppException appException)
            {
                context.Result = new JsonResult(new ApiResponseMessage
                {
                    Success = false,
                    Status_Code = appException.StatusCode,
                    Internel_Status_Code = appException.StatusCode,
                    Message = appException.Message,
                    Method_Name = actionName,
                    Data = appException.Details
                })
                {
                    StatusCode = StatusCodes.Status200OK
                };
                context.ExceptionHandled = true;
                return;
            }

            if (exception is OtpException otpException)
            {
                context.Result = new JsonResult(new ApiResponseMessage
                {
                    Success = false,
                    Status_Code = (int)HttpStatusCode.BadRequest,
                    Internel_Status_Code = (int)otpException.StatusCode,
                    Message = otpException.Message,
                    Method_Name = actionName,
                    Data = null
                })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                context.ExceptionHandled = true;
                return;
            }

            if (exception is KitchenException kitchenException)
            {
                context.Result = new JsonResult(new ApiResponseMessage
                {
                    Success = false,
                    Status_Code = (int)HttpStatusCode.BadRequest,
                    Internel_Status_Code = (int)kitchenException.StatusCode,
                    Message = kitchenException.Message,
                    Method_Name = actionName,
                    Data = null
                })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                context.ExceptionHandled = true;
                return;
            }

            if (exception is SmsDeliveryException smsException)
            {
                context.Result = new JsonResult(new ApiResponseMessage
                {
                    Success = false,
                    Status_Code = (int)HttpStatusCode.BadGateway,
                    Internel_Status_Code = (int)StatusInformation.Internel_Error,
                    Message = smsException.Message,
                    Method_Name = actionName,
                    Data = null
                })
                {
                    StatusCode = StatusCodes.Status502BadGateway
                };
                context.ExceptionHandled = true;
                return;
            }

            var errorMessage = exception.Message.Contains("incorrect syntax near", StringComparison.OrdinalIgnoreCase)
                ? "Please try after sometime or contact to support team"
                : "Something went wrong please contact to support team";

            var response = new ApiErrorResponseMessage
            {
                Success = false,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Exception_Code,
                Message = errorMessage,
                Error_Message = errorMessage,
                Error_Code = (int)StatusInformation.Exception_Code,
                Method_Name = actionName,
                Data = null
            };

            context.Result = new JsonResult(response)
            {
                StatusCode = StatusCodes.Status200OK
            };

            context.ExceptionHandled = true;
        }
    }
}
