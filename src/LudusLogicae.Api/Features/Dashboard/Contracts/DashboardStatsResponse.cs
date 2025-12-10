using System.Text.Json.Serialization;

namespace LudusLogicae.Api.Features.Dashboard.Contracts;

public class DashboardStatsResponse
{
    public MemberStats Members { get; set; } = new();
    public DebateStats Debates { get; set; } = new();
}

public class MemberStats
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Benched { get; set; }
    public int Retired { get; set; }
}

public class DebateStats
{
    public int Total { get; set; }
    public int Ready { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
}
