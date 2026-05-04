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

    private static string SanitizeForLog(string? value)
    {
        if (value is null) return string.Empty;
        var chars = new char[value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            chars[i] = char.IsControl(c) || c == '\u2028' || c == '\u2029' ? '_' : c;
        }
        return new string(chars);
    }
}