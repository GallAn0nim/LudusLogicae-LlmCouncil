using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Infrastructure.Llm.Models;
using LudusLogicae.Api.Infrastructure.Llm.OpenAI;

namespace LudusLogicae.Api.Infrastructure.Llm.Services;

public sealed class ProviderSelectingLLMClient : ILLMClient
{
    private readonly string _configuredProvider;
    private readonly ILogger<ProviderSelectingLLMClient> _logger;
    private readonly IServiceProvider _services;

    public ProviderSelectingLLMClient(IServiceProvider services, IConfiguration configuration,
        ILogger<ProviderSelectingLLMClient> logger)
    {
        _services = services;
        _logger = logger;
        _configuredProvider = (configuration["Llm:Provider"] ?? "OpenAI").Trim();
    }

    public Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var provider = string.IsNullOrWhiteSpace(request.Provider) ? _configuredProvider : request.Provider!;
        switch (provider.ToLowerInvariant())
        {
            case "openai":
                var openai = _services.GetRequiredService<OpenAIResponsesClient>();
                return openai.SendAsync(request, cancellationToken);
            default:
                _logger.LogError("Unknown LLM provider: {Provider}", provider);
                throw new LLMClientException($"Unknown LLM provider: '{provider}'", provider);
        }
    }
}
