namespace LudusLogicae.Api.Features.Metadata.Contracts;

public class MetadataInfoResponse
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Goals { get; set; } = new();
    public List<string> DebateFlow { get; set; } = new();
    public List<string> Principles { get; set; } = new();
    public StackInfo Stack { get; set; } = new();
}
