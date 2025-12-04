using LudusLogicae.Api.Core.Configuration;
using LudusLogicae.Api.Infrastructure.Llm.OpenAI;
using Microsoft.Extensions.Options;

namespace LudusLogicae.Api.Infrastructure.Llm.Services;

public interface ILlmProviderService
{
    string ValidateAndFallbackProvider(string provider);
    string ValidateAndFallbackModel(string provider, string model);
    bool IsProviderSupported(string provider);
    bool IsModelSupportedForProvider(string provider, string model);
    IReadOnlyList<string> GetSupportedProviders();
    IReadOnlyList<string> GetSupportedModelsForProvider(string provider);
}

public sealed class LlmProviderService : ILlmProviderService
{
    private const string DefaultFallbackProvider = "OpenAI";
    private readonly OpenAISettings _openAiSettings;
    private readonly LlmProvidersOptions _providersOptions;
    private readonly Random _random = new();

    public LlmProviderService(
        IOptions<LlmProvidersOptions> providersOptions,
        IOptions<OpenAISettings> openAiSettings)
    {
        _providersOptions = providersOptions.Value;
        _openAiSettings = openAiSettings.Value;
    }

    public string ValidateAndFallbackProvider(string provider)
    {
        return _providersOptions.IsProviderSupported(provider) ? provider : GetRandomSupportedProvider();
    }

    public string ValidateAndFallbackModel(string provider, string model)
    {
        return _providersOptions.IsModelSupported(provider, model)
            ? model
            : GetRandomSupportedModelForProvider(provider);
    }

    public bool IsProviderSupported(string provider)
    {
        return _providersOptions.IsProviderSupported(provider);
    }

    public bool IsModelSupportedForProvider(string provider, string model)
    {
        return _providersOptions.IsModelSupported(provider, model);
    }

    public IReadOnlyList<string> GetSupportedProviders()
    {
        return _providersOptions.GetProviderNames();
    }

    public IReadOnlyList<string> GetSupportedModelsForProvider(string provider)
    {
        return _providersOptions.GetModelsForProvider(provider);
    }

    private string GetRandomSupportedProvider()
    {
        var providers = _providersOptions.GetProviderNames();
        return providers.Count == 0 ? DefaultFallbackProvider : providers[_random.Next(providers.Count)];
    }

    private string GetRandomSupportedModelForProvider(string provider)
    {
        var models = _providersOptions.GetModelsForProvider(provider);
        return models.Count == 0 ? _openAiSettings.DefaultModel : models[_random.Next(models.Count)];
    }
}
