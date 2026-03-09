using System.Net;
using System.Text.Json;
using FluentValidation;
using Piedrazul.Domain.Exceptions;

namespace Piedrazul.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, errors) = ex switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Error de validación.",
                ve.Errors.Select(e => e.ErrorMessage).ToList()),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ex.Message,
                new List<string>()),

            SlotNotAvailableException => (
                HttpStatusCode.Conflict,
                ex.Message,
                new List<string>()),

            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                ex.Message,
                new List<string>()),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                ex.Message,
                new List<string>()),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                ex.Message,
                new List<string>()),

            _ => (
                HttpStatusCode.InternalServerError,
                $"Error interno: {ex.Message}",
                new List<string>())
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            errors
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}