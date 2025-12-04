namespace LudusLogicae.Api.Infrastructure.Llm.Models;

public sealed class LLMUsage
{
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int? TotalTokens { get; set; }
}
