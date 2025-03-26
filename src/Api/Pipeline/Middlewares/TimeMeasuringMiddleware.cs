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
        Log.Information($"Start Request: {httpContext.Request.Method} {httpContext.Request.Path}");
        var stopWatch = Stopwatch.StartNew();
        await _next(httpContext);
        stopWatch.Stop();
        var timeSpent = stopWatch.ElapsedMilliseconds;
        Log.Information($"End Request: {httpContext.Request.Method} {httpContext.Request.Path}");
        Log.Information($"Time Spent: {timeSpent} ms");
        Log.Information("--------------------------------------------------------------------");
    }
}