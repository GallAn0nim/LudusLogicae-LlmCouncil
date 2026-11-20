using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Infrastructure.Llm.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Processing;

public sealed class LlmResponseGenerator : ILlmResponseGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private readonly ILLMClient _llmClient;
    private readonly ILogger<LlmResponseGenerator> _logger;

    public LlmResponseGenerator(ILLMClient llmClient, ILogger<LlmResponseGenerator> logger)
    {
        _llmClient = llmClient;
        _logger = logger;
    }

    public async Task<PhaseGenerationResult> GenerateResponsesInParallelAsync<TInput>(
        List<PhaseRequest<TInput>> requestInputs,
        int maxOutputTokens,
        string phaseDescription,
        CancellationToken cancellationToken)
    {
        var successfulResponses = new ConcurrentBag<RawPhaseResult>();
        var failures = new ConcurrentBag<PhaseGenerationFailure>();

        var tasks = requestInputs.Select(async requestInput =>
        {
            var userContent = JsonSerializer.Serialize(requestInput.Input, JsonOptions);
            var request = CreateLlmRequest(
                requestInput.Provider,
                requestInput.Model,
                requestInput.Temperature,
                requestInput.SystemPrompt,
                userContent,
                maxOutputTokens);

            try
            {
                var response = await _llmClient.SendAsync(request, cancellationToken);
                var rawText = NormalizeLineEndings(response.Text ?? string.Empty);

                successfulResponses.Add(new RawPhaseResult(requestInput.SlotIndex, rawText));

                _logger.LogDebug(
                    "Successfully generated {Phase} response for slot {SlotIndex}",
                    phaseDescription,
                    requestInput.SlotIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate {Phase} response for slot {SlotIndex}. Error: {ErrorMessage}",
                    phaseDescription,
                    requestInput.SlotIndex,
                    ex.Message);

                failures.Add(new PhaseGenerationFailure(
                    requestInput.SlotIndex,
                    ex.Message,
                    ex));
            }
        });

        await Task.WhenAll(tasks);

        var result = new PhaseGenerationResult
        {
            SuccessfulResponses = successfulResponses.ToList(),
            Failures = failures.ToList()
        };

        if (result.IsCompleteFailure)
        {
            _logger.LogError(
                "Complete failure generating {Phase}: all {Count} responses failed",
                phaseDescription,
                result.TotalCount);
        }
        else if (!result.IsFullySuccessful)
        {
            _logger.LogWarning(
                "Partial failure generating {Phase}: {SuccessCount}/{TotalCount} responses succeeded, {FailureCount} failed",
                phaseDescription,
                result.SuccessfulResponses.Count,
                result.TotalCount,
                result.Failures.Count);
        }
        else
        {
            _logger.LogInformation(
                "Successfully generated all {Count} {Phase} responses",
                result.SuccessfulResponses.Count,
                phaseDescription);
        }

        return result;
    }

    private static LLMRequest CreateLlmRequest(string provider, string model, double temperature, string systemPrompt,
        string userContent, int maxTokens)
    {
        return new LLMRequest
        {
            Provider = provider,
            Model = model,
            Temperature = temperature,
            SystemPrompt = systemPrompt,
            Messages = [new LLMMessage { Role = LLMRole.User, Content = userContent }],
            MaxOutputTokens = maxTokens
        };
    }

    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\\n", "\n");
    }
}
