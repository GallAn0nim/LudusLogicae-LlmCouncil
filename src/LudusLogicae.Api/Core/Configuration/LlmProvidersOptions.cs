using System.ComponentModel.DataAnnotations;

namespace LudusLogicae.Api.Core.Configuration;

public sealed class LlmProvidersOptions : IValidatableObject
{
    public const string SectionName = "Llm";

    public string DefaultProvider { get; set; } = string.Empty;

    public List<LlmProviderConfig> Providers { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Providers.Count == 0)
        {
            yield return new ValidationResult(
                "At least one LLM provider must be configured.",
                [nameof(Providers)]);
        }

        if (!string.IsNullOrWhiteSpace(DefaultProvider) && !IsProviderSupported(DefaultProvider))
        {
            yield return new ValidationResult(
                $"DefaultProvider '{DefaultProvider}' is not in the Providers list.",
                [nameof(DefaultProvider)]);
        }

        foreach (var provider in Providers)
        {
            if (string.IsNullOrWhiteSpace(provider.Name))
            {
                yield return new ValidationResult(
                    "Provider name cannot be empty.",
                    [nameof(LlmProviderConfig.Name)]);
            }

            if (provider.SupportedModels.Count == 0)
            {
                yield return new ValidationResult(
                    $"Provider '{provider.Name}' must have at least one supported model.",
                    [nameof(LlmProviderConfig.SupportedModels)]);
            }

            if (!string.IsNullOrWhiteSpace(provider.DefaultModel) &&
                !provider.SupportedModels.Contains(provider.DefaultModel, StringComparer.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    $"Provider '{provider.Name}' DefaultModel '{provider.DefaultModel}' is not in SupportedModels.",
                    [nameof(LlmProviderConfig.DefaultModel)]);
            }
        }
    }

    public IReadOnlyList<string> GetProviderNames()
    {
        return Providers.Select(p => p.Name).ToList();
    }

    public string? GetDefaultModel(string providerName)
    {
        var provider =
            Providers.FirstOrDefault(p => string.Equals(p.Name, providerName, StringComparison.OrdinalIgnoreCase));
        if (provider is null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(provider.DefaultModel) ? provider.SupportedModels.FirstOrDefault() : provider.DefaultModel;
    }

    public IReadOnlyList<string> GetModelsForProvider(string providerName)
    {
        var provider =
            Providers.FirstOrDefault(p => string.Equals(p.Name, providerName, StringComparison.OrdinalIgnoreCase));
        return provider?.SupportedModels ?? [];
    }

    public bool IsProviderSupported(string providerName)
    {
        return Providers.Any(p => string.Equals(p.Name, providerName, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsModelSupported(string providerName, string modelName)
    {
        return FindSupportedModel(providerName, modelName) is not null;
    }

    public string? FindSupportedModel(string providerName, string modelName)
    {
        var provider =
            Providers.FirstOrDefault(p => string.Equals(p.Name, providerName, StringComparison.OrdinalIgnoreCase));
        return provider?.SupportedModels.FirstOrDefault(m => string.Equals(m, modelName, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class LlmProviderConfig
{
    public string Name { get; set; } = string.Empty;

    public string? DefaultModel { get; set; }

    public List<string> SupportedModels { get; set; } = [];
}
