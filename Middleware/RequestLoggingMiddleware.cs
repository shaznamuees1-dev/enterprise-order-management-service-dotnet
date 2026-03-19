using System.Diagnostics;
using System.Security.Claims;

namespace OrderManagementService.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation("Incoming Request: {Method} {Path} by User {UserId}",
            method, path, userId ?? "Anonymous");

        await _next(context);

        stopwatch.Stop();

        var statusCode = context.Response.StatusCode;

        _logger.LogInformation("Outgoing Response: {Method} {Path} responded {StatusCode} in {Elapsed} ms for User {UserId}",
            method, path, statusCode, stopwatch.ElapsedMilliseconds, userId ?? "Anonymous");
    }
}