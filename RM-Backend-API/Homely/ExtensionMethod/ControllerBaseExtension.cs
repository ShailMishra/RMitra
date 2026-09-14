using Microsoft.AspNetCore.Mvc;
using RMitra.Api.Common;
using RMitra.Api.Response;
using static RMitra.Api.Common.StatusMessage;

namespace RMitra.Api.ExtensionMethod;

public static class ControllerBaseExtension
{
    public static IActionResult OkCustom(this ControllerBase controller, object? input, object? data, ILogger? logger = null, string? message = null)
    {
        return controller.Ok(new ApiResponseMessage
        {
            Message = message ?? StatusInformation.Success.GetDisplayName(),
            Success = true,
            Internel_Status_Code = (int)StatusInformation.Success,
            Status_Code = StatusCodes.Status200OK,
            Data = data,
            Method_Name = controller.ControllerContext.ActionDescriptor.ActionName
        });
    }

    public static IActionResult Fail(this ControllerBase controller, object? input, object? data, ILogger? logger = null)
    {
        return controller.Ok(new ApiResponseMessage
        {
            Message = StatusInformation.Fail.GetDisplayName(),
            Success = false,
            Internel_Status_Code = (int)StatusInformation.Fail,
            Status_Code = StatusCodes.Status200OK,
            Data = data,
            Method_Name = controller.ControllerContext.ActionDescriptor.ActionName
        });
    }

    public static IActionResult FailCustom(this ControllerBase controller, object? input, object? data, ILogger? logger, string message)
    {
        return controller.Ok(new ApiResponseMessage
        {
            Message = string.IsNullOrWhiteSpace(message) ? StatusInformation.Fail.GetDisplayName() : message,
            Success = false,
            Internel_Status_Code = (int)StatusInformation.Fail,
            Status_Code = StatusCodes.Status200OK,
            Data = data,
            Method_Name = controller.ControllerContext.ActionDescriptor.ActionName
        });
    }

    public static IActionResult BadRequestCustom(this ControllerBase controller, object? input, object? data, ILogger? logger = null)
    {
        return controller.Ok(new ApiResponseMessage
        {
            Message = StatusInformation.Request_JSON_Body_Is_Null.GetDisplayName(),
            Success = false,
            Internel_Status_Code = (int)StatusInformation.Request_JSON_Body_Is_Null,
            Status_Code = StatusCodes.Status200OK,
            Data = data,
            Method_Name = controller.ControllerContext.ActionDescriptor.ActionName
        });
    }

    public static IActionResult NotFoundCustom(this ControllerBase controller, object? input, object? data, ILogger? logger = null)
    {
        return controller.Ok(new ApiResponseMessage
        {
            Message = StatusInformation.Fail.GetDisplayName(),
            Success = false,
            Internel_Status_Code = (int)StatusInformation.Fail,
            Status_Code = StatusCodes.Status200OK,
            Data = data,
            Method_Name = controller.ControllerContext.ActionDescriptor.ActionName
        });
    }
}
