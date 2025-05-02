using Api.Common.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid) context.Result = new ValidationFailedResult(context.ModelState);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        // Method intentionally left empty.
    }
}