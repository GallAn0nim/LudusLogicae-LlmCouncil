namespace LudusLogicae.Api.Infrastructure.Llm.Models;

public sealed record LLMRequest
{
    public string? SystemPrompt { get; set; }
    public List<LLMMessage> Messages { get; set; } = new();

    public string? Provider { get; set; }

    public string? Model { get; set; }
    public double? Temperature { get; set; }
    public int? MaxOutputTokens { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}
