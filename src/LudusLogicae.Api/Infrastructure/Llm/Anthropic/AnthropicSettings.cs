using System.ComponentModel.DataAnnotations;

namespace LudusLogicae.Api.Infrastructure.Llm.Anthropic;

public sealed class AnthropicSettings
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = string.Empty;

    [Range(10, 600)] public int TimeoutSeconds { get; set; } = 300;

    [Range(0, 10)] public int MaxRetries { get; set; } = 3;
}
