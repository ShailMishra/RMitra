using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using RM.Infrastructure.CommonClass;
using RMitra.Api.Common;
using RMitra.Api.Response;
using static RMitra.Api.Common.StatusMessage;

namespace RMitra.Api.ActionFilters;

public class CustomAuthenticationFilter : Attribute, IAuthorizationFilter
{
    private readonly TokenSettings _tokenSettings;

    public CustomAuthenticationFilter(IConfiguration configuration)
    {
        _tokenSettings = configuration.GetSection("TokenSettings").Get<TokenSettings>() ?? new TokenSettings();
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

        var apiKey = FirstHeader(context, "API_Key", "api_key");
        var secretKey = FirstHeader(context, "Secret_Key", "secret_key");

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

        if (!_tokenSettings.Matches(apiKey, secretKey))
        {
            context.Result = Fail(actionName, StatusInformation.API_Key_Is_Secret_Key_Invalid);
        }
    }

    private static string FirstHeader(AuthorizationFilterContext context, params string[] names)
    {
        foreach (var name in names)
        {
            if (context.HttpContext.Request.Headers.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
                return value.ToString();
        }

        return string.Empty;
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
