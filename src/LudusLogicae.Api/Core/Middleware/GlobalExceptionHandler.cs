using System.Diagnostics;
using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Shared.Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Core.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        logger.LogError(
            exception,
            "An unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
            traceId,
            context.Request.Path);

        var (statusCode, response) = exception switch
        {

            LLMClientException llmEx => HandleLLMException(llmEx, traceId),

            TaskCanceledException or OperationCanceledException => HandleCancelledException(traceId),
            TimeoutException timeout => HandleTimeoutException(timeout, traceId),

            DbUpdateConcurrencyException concurrencyEx => HandleConcurrencyException(concurrencyEx, traceId),
            DbUpdateException dbEx => HandleDatabaseException(dbEx, traceId),

            ArgumentNullException argNull => HandleArgumentException(argNull, traceId),
            ArgumentException argEx => HandleArgumentException(argEx, traceId),
            InvalidOperationException invEx => HandleInvalidOperationException(invEx, traceId),

            _ => HandleGenericException(exception, traceId)
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new { response }, cancellationToken);

        return true;
    }

    private static (int StatusCode, LudusLogicaeResponse<object> Response) HandleLLMException(
        LLMClientException exception,
        string traceId)
    {
        var (statusCode, envelope) = ResponseFactory.BadGateway<object>(
            $"LLM provider error ({exception.Provider}): {exception.Message}",
            exception);

        return (statusCode, envelope);
    }

    private static (int StatusCode, LudusLogicaeResponse<object> Response) HandleCancelledException(string traceId)
    {
        var (statusCode, envelope) = ResponseFactory.BadRequest<object>("Request was cancelled.");
        return (499, envelope);
    }

    private static (int StatusCode, LudusLogicaeResponse<object> Response) HandleArgumentException(
        ArgumentException exception,
        string traceId)
    {
        var (statusCode, envelope) = ResponseFactory.BadRequest<object>(exception.Message);
        return (statusCode, envelope);
    }

    private static (int StatusCode, LudusLogicaeResponse<object> Response) HandleInvalidOperationException(
        InvalidOperationException exception,
        string traceId)
    {
        var (statusCode, envelope) = ResponseFactory.BadRequest<object>(exception.Message);
        return (statusCode, envelope);
    }

    private static (int StatusCode, LudusLogicaeResponse<object> Response) HandleTimeoutException(
        TimeoutException exception,
        string traceId)
    {
        var (statusCode, envelope) = ResponseFactory.InternalServerError<object>(
            "The request timed out. Please try again.",
            exception);
        return (504, envelope);
    }

    private static (int StatusCode, LudusLogicaeResponse<object> Response) HandleDatabaseException(
        DbUpdateException exception,
        string traceId)
    {

        var isConflict = exception.InnerException?.Message.Contains("duplicate key") == true ||
                         exception.InnerException?.Message.Contains("unique constraint") == true ||
                         exception.InnerException?.Message.Contains("UNIQUE constraint") == true;

        if (isConflict)
        {
            var (statusCode, envelope) = ResponseFactory.BadRequest<object>(
                "The operation conflicts with existing data. Please check your input.");
            return (409, envelope);
        }

        var (genericStatusCode, genericEnvelope) = ResponseFactory.InternalServerError<object>(
            "A database error occurred while processing your request.",
            exception);
        return (genericStatusCode, genericEnvelope);
    }

    private static (int StatusCode, LudusLogicaeResponse<object> Response) HandleConcurrencyException(
        DbUpdateConcurrencyException exception,
        string traceId)
    {
        var (statusCode, envelope) = ResponseFactory.BadRequest<object>(
            "The data has been modified by another user. Please refresh and try again.");
        return (409, envelope);
    }

    private static (int StatusCode, LudusLogicaeResponse<object> Response) HandleGenericException(
        Exception exception,
        string traceId)
    {
        var (statusCode, envelope) = ResponseFactory.InternalServerError<object>(
            "An unexpected error occurred while processing your request.",
            exception);

        return (statusCode, envelope);
    }
}
