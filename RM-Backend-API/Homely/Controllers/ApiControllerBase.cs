using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;

namespace RMitra.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected string? IdempotencyKey => Request.Headers["Idempotency-Key"].FirstOrDefault();

    protected IActionResult OkData(object? data, string? message = null) =>
        this.OkCustom(null, data, message: message);

    protected IActionResult FailData(object? data = null) =>
        this.Fail(null, data);

    protected IActionResult BadBody(object? input = null) =>
        this.BadRequestCustom(input, null);
}
