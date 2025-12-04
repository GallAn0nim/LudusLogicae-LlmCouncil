using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Infrastructure.Llm.Models;
using Polly;
using Polly.CircuitBreaker;

namespace LudusLogicae.Api.Infrastructure.Llm.OpenAI;

public sealed class ResilientOpenAIClient : ILLMClient
{
    private readonly IAsyncPolicy<LLMResponse> _circuitBreakerPolicy;
    private readonly OpenAIResponsesClient _innerClient;
    private readonly ILogger<ResilientOpenAIClient> _logger;

    public ResilientOpenAIClient(
        OpenAIResponsesClient innerClient,
        ILogger<ResilientOpenAIClient> logger)
    {
        _innerClient = innerClient;
        _logger = logger;

        _circuitBreakerPolicy = Policy<LLMResponse>
            .Handle<LLMClientException>()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromSeconds(30),
                (result, duration) =>
                {
                    _logger.LogWarning(
                        "Circuit breaker opened due to consecutive failures. Will retry after {Duration}s",
                        duration.TotalSeconds);
                },
                () => { _logger.LogInformation("Circuit breaker reset. Resuming normal operation."); },
                () => { _logger.LogInformation("Circuit breaker testing if service has recovered."); });
    }

    public async Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _circuitBreakerPolicy.ExecuteAsync(
                async ct => await _innerClient.SendAsync(request, ct),
                cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(
                ex,
                "Circuit breaker is open. LLM provider appears to be unavailable.");
            throw new LLMClientException(
                "LLM service is temporarily unavailable due to repeated failures. Please try again later.",
                "OpenAI",
                inner: ex);
        }
    }
}
