
namespace Api.Pipeline.Middlewares;

public class RemoveUnauthorizedHeaderMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.Any(x => x.Key == "Authorization-UserId"))
            httpContext.Request.Headers.Remove("Authorization-UserId");

        if (httpContext.Request.Headers.Any(x => x.Key == "Authorization-UserType"))
            httpContext.Request.Headers.Remove("Authorization-UserType");

        if (httpContext.Request.Headers.Any(x => x.Key == "Authorization-AccountStatus"))
            httpContext.Request.Headers.Remove("Authorization-AccountStatus");

        if (httpContext.Request.Headers.Any(x => x.Key == "Authorization-SchoolId"))
            httpContext.Request.Headers.Remove("Authorization-SchoolId");
        await next(httpContext);
    }
}