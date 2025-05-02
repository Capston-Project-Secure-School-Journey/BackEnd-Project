using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Api.Common.Utilities;
using Api.Common.Exceptions;

namespace Api.Pipeline.Middlewares;

public class ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
{
    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
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
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Message = exception.Message
        };
        switch (exception)
        {
            case NotFoundException e:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorDetail.Message = e.Message;
                break;
            case UnAuthorizedException e:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorDetail.Message = e.Message;
                break;
            case ForbiddenException e:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorDetail.Message = e.Message;
                break;
            case BadRequestException e:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorDetail.Message = e.Message;
                break;
            case ValidationException e:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorDetail.Message = e.Message;
                break;
            default:
                logger.LogError(exception,"Unhandled exception");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorDetail.Message = "Internal Server Error";
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