using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using RMitra.Api.Common;
using RMitra.Api.Response;
using static RMitra.Api.Common.StatusMessage;

namespace RMitra.Api.ActionFilters;

public class CustomAuthenticationFilter : Attribute, IAuthorizationFilter
{
    private readonly string _apiKey;
    private readonly string _secretKey;

    public CustomAuthenticationFilter(IConfiguration configuration)
    {
        _apiKey = configuration.GetSection("TokenSettings:API_Key").Value ?? string.Empty;
        _secretKey = configuration.GetSection("TokenSettings:Secret_Key").Value ?? string.Empty;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var controllerName = descriptor?.ControllerName ?? string.Empty;
        var actionName = descriptor?.ActionName ?? string.Empty;

        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api/homely", StringComparison.OrdinalIgnoreCase))
            return;

        if (string.Equals(controllerName, "Health", StringComparison.OrdinalIgnoreCase))
            return;

        context.HttpContext.Request.Headers.TryGetValue("API_Key", out var apiKeyHeader);
        context.HttpContext.Request.Headers.TryGetValue("Secret_Key", out var secretKeyHeader);

        var apiKey = apiKeyHeader.ToString();
        var secretKey = secretKeyHeader.ToString();

        if (string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(secretKey))
        {
            context.Result = Fail(actionName, StatusInformation.API_Key_Secret_Key_Is_Null);
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Result = Fail(actionName, StatusInformation.API_Key_Is_Null);
            return;
        }

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            context.Result = Fail(actionName, StatusInformation.Secret_Key_Is_Null);
            return;
        }

        if (!string.Equals(apiKey, _apiKey, StringComparison.Ordinal) ||
            !string.Equals(secretKey, _secretKey, StringComparison.Ordinal))
        {
            context.Result = Fail(actionName, StatusInformation.API_Key_Is_Secret_Key_Invalid);
        }
    }

    private static JsonResult Fail(string actionName, StatusInformation status)
    {
        return new JsonResult(new ApiResponseMessage
        {
            Success = false,
            Status_Code = (int)HttpStatusCode.OK,
            Internel_Status_Code = (int)status,
            Message = status.GetDisplayName(),
            Method_Name = actionName,
            Data = null
        })
        {
            StatusCode = StatusCodes.Status200OK
        };
    }
}
