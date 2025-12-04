namespace LudusLogicae.Api.Infrastructure.Llm.Models;

public sealed class LLMResponse
{
    public string? Text { get; set; }
    public List<LLMToolCall>? ToolCalls { get; set; }
    public LLMUsage? Usage { get; set; }
    public string? FinishReason { get; set; }

    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTimeOffset? ProviderCreatedAt { get; set; }

    public string? RawJson { get; set; }
}

public sealed class LLMToolCall
{
    public string Name { get; set; } = string.Empty;
    public string? ArgumentsJson { get; set; }
}
