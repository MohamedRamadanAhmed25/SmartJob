namespace SmartJob.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startedAt = DateTime.UtcNow;
        await _next(context);
        var elapsed = DateTime.UtcNow - startedAt;

        _logger.LogInformation(
            "{Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsed.TotalMilliseconds);
    }
}
