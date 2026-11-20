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

        var result = exception switch
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

        context.Response.StatusCode = result.StatusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new { response = result.Envelope }, cancellationToken);

        return true;
    }

    private static ServiceResult<object> HandleLLMException(
        LLMClientException exception,
        string traceId)
    {
        return ResponseFactory.BadGateway<object>(
            $"LLM provider error ({exception.Provider}): {exception.Message}",
            exception);
    }

    private static ServiceResult<object> HandleCancelledException(string traceId)
    {
        return ResponseFactory.BadRequest<object>("Request was cancelled.") with { StatusCode = 499 };
    }

    private static ServiceResult<object> HandleArgumentException(
        ArgumentException exception,
        string traceId)
    {
        return ResponseFactory.BadRequest<object>(exception.Message);
    }

    private static ServiceResult<object> HandleInvalidOperationException(
        InvalidOperationException exception,
        string traceId)
    {
        return ResponseFactory.BadRequest<object>(exception.Message);
    }

    private static ServiceResult<object> HandleTimeoutException(
        TimeoutException exception,
        string traceId)
    {
        return ResponseFactory.InternalServerError<object>(
            "The request timed out. Please try again.",
            exception) with { StatusCode = 504 };
    }

    private static ServiceResult<object> HandleDatabaseException(
        DbUpdateException exception,
        string traceId)
    {
        var isConflict = exception.InnerException?.Message.Contains("duplicate key") == true ||
                         exception.InnerException?.Message.Contains("unique constraint") == true ||
                         exception.InnerException?.Message.Contains("UNIQUE constraint") == true;

        if (isConflict)
        {
            return ResponseFactory.BadRequest<object>(
                "The operation conflicts with existing data. Please check your input.") with { StatusCode = 409 };
        }

        return ResponseFactory.InternalServerError<object>(
            "A database error occurred while processing your request.",
            exception);
    }

    private static ServiceResult<object> HandleConcurrencyException(
        DbUpdateConcurrencyException exception,
        string traceId)
    {
        return ResponseFactory.BadRequest<object>(
            "The data has been modified by another user. Please refresh and try again.") with { StatusCode = 409 };
    }

    private static ServiceResult<object> HandleGenericException(
        Exception exception,
        string traceId)
    {
        return ResponseFactory.InternalServerError<object>(
            "An unexpected error occurred while processing your request.",
            exception);
    }
}
