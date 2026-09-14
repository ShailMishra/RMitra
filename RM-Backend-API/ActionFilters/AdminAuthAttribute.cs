using RM.Infrastructure.CommonClass;
using RM.Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using static RM.Infrastructure.CommonClass.StatusMessage;

namespace RM_Backend_API.ActionFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var tokenSettings = context.HttpContext.RequestServices.GetRequiredService<TokenSettings>();
            var apiKey = context.HttpContext.Request.Headers["api_key"].FirstOrDefault();
            var secretKey = context.HttpContext.Request.Headers["secret_key"].FirstOrDefault();

            StatusInformation statusCode;
            string message;

            if (string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(secretKey))
            {
                statusCode = StatusInformation.API_Key_Secret_Key_Is_Null;
                message = "API Key and Secret Key are required for admin access.";
            }
            else if (string.IsNullOrWhiteSpace(apiKey))
            {
                statusCode = StatusInformation.API_Key_Is_Null;
                message = "API Key is required for admin access.";
            }
            else if (string.IsNullOrWhiteSpace(secretKey))
            {
                statusCode = StatusInformation.Secret_Key_Is_Null;
                message = "Secret Key is required for admin access.";
            }
            else if (!string.Equals(apiKey, tokenSettings.API_Key, StringComparison.Ordinal) ||
                     !string.Equals(secretKey, tokenSettings.Secret_Key, StringComparison.Ordinal))
            {
                statusCode = StatusInformation.API_Key_Is_Secret_Key_Invalid;
                message = "Invalid API Key or Secret Key.";
            }
            else
            {
                return;
            }

            var actionName = (context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)?.ActionName ?? string.Empty;

            context.Result = new JsonResult(new ApiResponseMessage
            {
                Success = false,
                Status_Code = (int)HttpStatusCode.Unauthorized,
                Internel_Status_Code = (int)statusCode,
                Message = message,
                Method_Name = actionName,
                Data = null
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}
