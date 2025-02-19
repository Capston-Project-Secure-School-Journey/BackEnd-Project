using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Api.Common.Utilities
{
    public class ValidationError(string field, string message)
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Field { get; } = field != string.Empty ? field : null;

        public string Message { get; } = message;
    }

    public class ValidationResultModel
    {
        public string Message { get; }
        public List<ValidationError> Errors { get; }
        public ValidationResultModel(ModelStateDictionary modelState)
        {
            Message = Constants.VALIDATION_FAILED;
            Errors = modelState.Keys
                .SelectMany(key => modelState[key]!.Errors.Select(x => new ValidationError(key, x.ErrorMessage.ToString())))
                .ToList();
        }
    }

    public class ValidationFailedResult : ObjectResult
    {
        public ValidationFailedResult(ModelStateDictionary modelState)
            : base(new ValidationResultModel(modelState))
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity;
        }
    }

    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ValidationFailedResult(context.ModelState);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context) { }
    }
}