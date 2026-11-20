using LudusLogicae.Api.Core.Configuration;
using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
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
    string? GetDefaultProvider();
}

public sealed class LlmProviderService : ILlmProviderService
{
    private readonly IServiceProviderIsKeyedService _keyedServices;
    private readonly LlmProvidersOptions _providersOptions;

    public LlmProviderService(
        IOptions<LlmProvidersOptions> providersOptions,
        IServiceProviderIsKeyedService keyedServices)
    {
        _providersOptions = providersOptions.Value;
        _keyedServices = keyedServices;
    }

    public string ValidateAndFallbackProvider(string provider)
    {
        return IsProviderSupported(provider) ? provider : GetDefaultProvider() ?? provider;
    }

    public string ValidateAndFallbackModel(string provider, string model)
    {
        return _providersOptions.FindSupportedModel(provider, model)
               ?? _providersOptions.GetDefaultModel(provider)
               ?? model;
    }

    public bool IsProviderSupported(string provider)
    {
        return !string.IsNullOrWhiteSpace(provider)
               && _providersOptions.IsProviderSupported(provider)
               && _keyedServices.IsKeyedService(typeof(ILLMClient), ProviderSelectingLLMClient.Key(provider));
    }

    public bool IsModelSupportedForProvider(string provider, string model)
    {
        return IsProviderSupported(provider) && _providersOptions.IsModelSupported(provider, model);
    }

    public IReadOnlyList<string> GetSupportedProviders()
    {
        return _providersOptions.GetProviderNames().Where(IsProviderSupported).ToList();
    }

    public IReadOnlyList<string> GetSupportedModelsForProvider(string provider)
    {
        return IsProviderSupported(provider) ? _providersOptions.GetModelsForProvider(provider) : [];
    }

    public string? GetDefaultProvider()
    {
        return IsProviderSupported(_providersOptions.DefaultProvider)
            ? _providersOptions.DefaultProvider
            : GetSupportedProviders().FirstOrDefault();
    }
}
