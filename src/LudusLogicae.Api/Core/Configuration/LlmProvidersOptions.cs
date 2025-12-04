using System.ComponentModel.DataAnnotations;

namespace LudusLogicae.Api.Core.Configuration;

public sealed class LlmProvidersOptions : IValidatableObject
{
    public const string SectionName = "Llm";

    public List<LlmProviderConfig> Providers { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Providers.Count == 0)
            yield return new ValidationResult(
                "At least one LLM provider must be configured.",
                [nameof(Providers)]);

        foreach (var provider in Providers)
        {
            if (string.IsNullOrWhiteSpace(provider.Name))
                yield return new ValidationResult(
                    "Provider name cannot be empty.",
                    [nameof(LlmProviderConfig.Name)]);

            if (provider.SupportedModels.Count == 0)
                yield return new ValidationResult(
                    $"Provider '{provider.Name}' must have at least one supported model.",
                    [nameof(LlmProviderConfig.SupportedModels)]);
        }
    }

    public IReadOnlyList<string> GetProviderNames()
    {
        return Providers.Select(p => p.Name).ToList();
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
        var provider =
            Providers.FirstOrDefault(p => string.Equals(p.Name, providerName, StringComparison.OrdinalIgnoreCase));
        return provider?.SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase) ?? false;
    }
}

public sealed class LlmProviderConfig
{
    public string Name { get; set; } = string.Empty;

    public List<string> SupportedModels { get; set; } = [];
}
