using System.Text.Json;
using Chroma.Application.Common.Exceptions;
using Chroma.Application.Common.Responses;
using Chroma.Localization;

namespace Chroma.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment,
    IApiMessageLocalizer localizer)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

        var (statusCode, messageCode, fallbackMessage) = exception switch
        {
            AppException appException => (
                appException.StatusCode,
                appException.MessageCode,
                appException.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, (string?)null, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "auth.unauthorized", exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "common.notFound", exception.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, (string?)null, exception.Message),
            NotSupportedException => (StatusCodes.Status400BadRequest, (string?)null, exception.Message),
            _ => (StatusCodes.Status500InternalServerError,
                "common.unexpected",
                environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.")
        };

        if (context.Response.HasStarted)
        {
            throw exception;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var message = localizer.Localize(
            messageCode,
            fallbackMessage,
            LanguageCodeMiddleware.GetLanguage(context));
        var response = messageCode is null
            ? ApiResponse.Fail(message)
            : ApiResponse.Fail(messageCode, message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
