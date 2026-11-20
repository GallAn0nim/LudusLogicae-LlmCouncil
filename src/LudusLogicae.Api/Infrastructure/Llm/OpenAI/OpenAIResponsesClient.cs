using System.Text.Json;
using System.Text.Json.Serialization;
using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Infrastructure.Llm.Models;
using LudusLogicae.Api.Infrastructure.Llm.OpenAI.Strategies;
using Microsoft.Extensions.Options;
using OpenAI;

namespace LudusLogicae.Api.Infrastructure.Llm.OpenAI;

public sealed class OpenAIResponsesClient : ILLMClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ILogger<OpenAIResponsesClient> _logger;
    private readonly OpenAIClient _openAIClient;
    private readonly OpenAISettings _options;
    private readonly IEnumerable<IOpenAIModelStrategy> _strategies;

    public OpenAIResponsesClient(
        OpenAIClient openAIClient,
        IOptions<OpenAISettings> options,
        ILogger<OpenAIResponsesClient> logger,
        IEnumerable<IOpenAIModelStrategy> strategies)
    {
        _openAIClient = openAIClient;
        _options = options.Value;
        _logger = logger;
        _strategies = strategies;
    }

    public async Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.DefaultModel : request.Model!;

        var strategy = GetStrategy(model);

        var attempt = 0;
        var maxRetries = Math.Max(0, _options.MaxRetries);
        var baseDelay = Math.Max(50, _options.RetryBaseDelayMs);
        Exception? lastEx = null;

        while (attempt <= maxRetries)
        {
            try
            {
                return await strategy.ExecuteAsync(_openAIClient, request, cancellationToken);
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                lastEx = ex;
                _logger.LogWarning(ex, "Attempt {Attempt} failed for model {Model}. Retrying...", attempt + 1, model);
                attempt++;
                await Task.Delay(baseDelay * (int)Math.Pow(2, attempt - 1), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "All {Retries} attempts failed for model {Model}.", maxRetries + 1, model);
                throw new LLMClientException(
                    $"Failed to get response from OpenAI for model {model} after {maxRetries + 1} attempts.", "OpenAI",
                    inner: ex);
            }
        }

        throw new LLMClientException("Unexpected flow in SendAsync", "OpenAI");
    }

    private IOpenAIModelStrategy GetStrategy(string model)
    {
        foreach (var strategy in _strategies)
        {
            if (strategy.CanHandle(model))
            {
                return strategy;
            }
        }

        throw new InvalidOperationException($"No strategy found for model: {model}");
    }
}
