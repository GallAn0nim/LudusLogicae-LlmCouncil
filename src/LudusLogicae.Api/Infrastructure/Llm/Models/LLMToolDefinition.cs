using System.Text.Json.Nodes;

namespace LudusLogicae.Api.Infrastructure.Llm.Models;

public sealed class LLMToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public JsonObject? JsonSchema { get; set; }
}
