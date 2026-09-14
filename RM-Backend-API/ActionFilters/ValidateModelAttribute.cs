using RM.Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using static RM.Infrastructure.CommonClass.StatusMessage;

namespace RM_Backend_API.ActionFilters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = descriptor?.ActionName ?? string.Empty;

                foreach (var key in context.ModelState.Keys.ToList())
                {
                    context.ModelState.Clear();
                    context.ModelState.AddModelError(key, "Please ensure all the fields are correctly formatted.");
                }

                var response = new ApiResponseMessage
                {
                    Status_Code = (int)HttpStatusCode.BadRequest,
                    Internel_Status_Code = (int)StatusInformation.Fail,
                    Message = "Something went wrong please contact to support team",
                    Success = false,
                    Method_Name = actionName,
                    Data = null,
                    Model_State = context.ModelState
                };

                context.Result = new JsonResult(response);
            }

            base.OnActionExecuting(context);
        }
    }
}
