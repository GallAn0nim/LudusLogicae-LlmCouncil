namespace LudusLogicae.Api.Infrastructure.Llm.Models;

public sealed class LLMMessage
{
    public LLMRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
}
