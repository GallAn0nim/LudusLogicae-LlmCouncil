using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Contracts;

public class DebateResponse
{
    public Guid Id { get; set; }
    public DebatePhase Status { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public int NMembers { get; set; }
    public int NRounds { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DebateState? State { get; set; }
}
