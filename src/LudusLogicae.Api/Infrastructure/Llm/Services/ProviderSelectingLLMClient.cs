using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Infrastructure.Llm.Models;

namespace LudusLogicae.Api.Infrastructure.Llm.Services;

public sealed class ProviderSelectingLLMClient : ILLMClient
{
    private readonly ILogger<ProviderSelectingLLMClient> _logger;
    private readonly ILlmProviderService _providerService;
    private readonly IServiceProvider _services;

    public ProviderSelectingLLMClient(IServiceProvider services, ILlmProviderService providerService,
        ILogger<ProviderSelectingLLMClient> logger)
    {
        _services = services;
        _providerService = providerService;
        _logger = logger;
    }

    public static string Key(string provider)
    {
        return provider.Trim().ToLowerInvariant();
    }

    public Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var provider = request.Provider;
        if (string.IsNullOrWhiteSpace(provider) || !_providerService.IsProviderSupported(provider))
        {
            var fallback = _providerService.GetDefaultProvider();
            if (!string.IsNullOrWhiteSpace(provider) && fallback is not null)
            {
                _logger.LogWarning("LLM provider {Provider} unavailable, using default {Default}", provider, fallback);
            }

            provider = fallback ?? provider;
            request.Provider = provider;
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            _logger.LogError("No LLM provider is configured");
            throw new LLMClientException(
                "No LLM provider is configured. Set an API key for at least one provider (e.g. OpenAI:ApiKey).",
                "none");
        }

        var model = _providerService.ValidateAndFallbackModel(provider, request.Model ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(request.Model) && model != request.Model)
        {
            _logger.LogWarning("Model {Model} is not supported by {Provider}, using {Fallback}", request.Model,
                provider, model);
        }

        request.Model = model;

        var client = _services.GetKeyedService<ILLMClient>(Key(provider));
        if (client is null)
        {
            _logger.LogError("LLM provider not configured: {Provider}", provider);
            throw new LLMClientException($"LLM provider '{provider}' is not configured.", provider);
        }

        return client.SendAsync(request, cancellationToken);
    }
}
