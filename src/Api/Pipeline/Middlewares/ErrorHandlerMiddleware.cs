using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Api.Common.Utilities;
using Api.Common.Utilities.Exceptions;

namespace Api.Pipeline.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {

            var response = context.Response;

            if (response.HasStarted)
                return;
            response.ContentType = "application/json";

            var errorDetail = new ErrorDetails()
            {
                StatusCode = (int)System.Net.HttpStatusCode.InternalServerError,
                Message = exception.Message
            };
            switch (exception)
            {
                case NotFoundException e:
                    // Custom not found
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case UnAuthorizedException e:
                    // Custom not found
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case ForbiddenException e:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case BadRequestException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case ValidationException e:
                    response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    errorDetail.Message = e.Message;
                    break;
                default:
                    // unhandled error
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorDetail.Message = exception.Message;
                    break;
            }

            errorDetail.StatusCode = response.StatusCode;
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            await response.WriteAsync(JsonSerializer.Serialize(errorDetail, serializeOptions));
        }
    }
}