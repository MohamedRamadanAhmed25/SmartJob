using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.Exceptions;

namespace SmartJob.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Request failed for {Method} {Path}", context.Request.Method, context.Request.Path);

        var (statusCode, message) = exception switch
        {
            ApiException apiException => (apiException.StatusCode, apiException.Message),
            ValidationException validationException => ((int)HttpStatusCode.BadRequest, validationException.Message),
            DbUpdateException dbUpdateException => ((int)HttpStatusCode.InternalServerError, GetDbMessage(dbUpdateException)),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, exception.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        if (_environment.IsDevelopment())
        {
            await context.Response.WriteAsJsonAsync(new
            {
                error = message,
                statusCode,
                exception = exception.GetType().Name,
                detail = exception.InnerException?.Message ?? exception.Message
            });
            return;
        }

        await context.Response.WriteAsJsonAsync(new
        {
            error = message,
            statusCode
        });
    }

    private string GetDbMessage(DbUpdateException exception)
    {
        if (_environment.IsDevelopment())
        {
            return exception.InnerException?.Message ?? exception.Message;
        }

        return "A database error occurred.";
    }
}
