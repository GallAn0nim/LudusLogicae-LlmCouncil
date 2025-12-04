namespace LudusLogicae.Api.Core.Configuration;

public sealed class HealthCheckOptions
{
    public const string SectionName = "HealthChecks";

    public string Path { get; set; } = "/health-check";
}
