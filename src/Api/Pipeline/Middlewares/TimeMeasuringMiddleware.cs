using System.Diagnostics;
using Serilog;

namespace Api.Pipeline.Middlewares;

public class TimeMeasuringMiddleware
{
    private readonly RequestDelegate _next;

    public TimeMeasuringMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var stopWatch = Stopwatch.StartNew();
        await _next(httpContext);
        stopWatch.Stop();
        var timeSpent = stopWatch.ElapsedMilliseconds;
        Log.Information("Request: {Method} {Path},Time Spent: {TimeSpent} ms",
            httpContext.Request.Method,
            httpContext.Request.Path,
            timeSpent);
    }
}