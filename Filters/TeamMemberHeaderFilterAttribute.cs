using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aihrly.Filters;

public class TeamMemberHeaderFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var headerValue = context.HttpContext.Request.Headers["X-Team-Member-Id"].ToString();

        if (string.IsNullOrWhiteSpace(headerValue))
        {
            context.Result = new BadRequestObjectResult(new
            {
                title = "Missing team member identification",
                status = 400,
                detail = "The X-Team-Member-Id header is required for this request."
            });
            return;
        }

        if (!Guid.TryParse(headerValue, out _))
        {
            context.Result = new BadRequestObjectResult(new
            {
                title = "Invalid team member identification",
                status = 400,
                detail = "The X-Team-Member-Id header must be a valid GUID."
            });
            return;
        }
    }

    public override void OnActionExecuted(ActionExecutedContext context) { }
}
