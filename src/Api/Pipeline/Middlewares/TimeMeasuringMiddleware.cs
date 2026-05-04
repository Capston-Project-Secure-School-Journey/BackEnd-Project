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
        var method = SanitizeForLog(httpContext.Request.Method);
        var path = SanitizeForLog(httpContext.Request.Path.Value);
        Log.Information("Request: {Method} {Path},Time Spent: {TimeSpent} ms",
            method,
            path,
            timeSpent);
    }

    private static string SanitizeForLog(string? value) =>
        value?.Replace("\r", "\\r").Replace("\n", "\\n") ?? string.Empty;
}