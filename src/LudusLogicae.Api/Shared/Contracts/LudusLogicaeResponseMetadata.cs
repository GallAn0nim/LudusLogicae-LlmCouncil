namespace LudusLogicae.Api.Shared.Contracts;

public sealed class LudusLogicaeResponseMetadata
{
    public string StatusCode { get; init; } = "200";

    public string StatusDescription { get; init; } = "OK";

    public string[] Errors { get; init; } = [];
}
