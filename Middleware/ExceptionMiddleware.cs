using System.Net;
using OrderManagementService.DTOs;

namespace OrderManagementService.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, ex.Message, HttpStatusCode.BadRequest);
        }
        catch (Exception)
        {
            await HandleExceptionAsync(context, "An unexpected error occurred.", HttpStatusCode.InternalServerError);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context,
        string message,
        HttpStatusCode statusCode)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Success = false,
            Message = message,
            StatusCode = (int)statusCode,
            Timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}