using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Entities;

public class Debate : IEntity
{
    public string Question { get; set; } = string.Empty;
    public string? Context { get; set; }
    public int NMembers { get; set; } = 5;
    public int NRounds { get; set; } = 3;

    public DebatePhase Phase { get; set; } = DebatePhase.Created;

    public Guid? PreviousAttemptId { get; set; }

    public List<BallotItem> Ballots { get; set; } = new();
    public DebateResult? Result { get; set; }

    public DebateState? State { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? FailedAt { get; set; }

    public List<DebateParticipant> Participants { get; set; } = new();
    public Guid Id { get; set; } = Guid.NewGuid();
}

public record BallotItem(int VoterSlot, List<int> RankedSlots);

public record DebateResult(int WinnerSlot, string Summary);
