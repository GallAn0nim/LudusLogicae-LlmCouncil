using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Data;
using LudusLogicae.Api.Infrastructure.Data;

namespace LudusLogicae.Api.Features.Debates.Services;

public sealed class DebateStateManager(
    IDebateRepository repository,
    AppDbContext dbContext) : IDebateStateManager
{
    public async Task PersistStateAsync(
        Debate debate,
        DebateState state,
        CancellationToken cancellationToken)
    {
        debate.StartedAt ??= DateTimeOffset.UtcNow;

        debate.State = state;
        dbContext.Entry(debate).Property(d => d.State).IsModified = true;
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task PersistFinalStateAsync(
        Debate debate,
        DebateState state,
        CancellationToken cancellationToken)
    {
        debate.Phase = DebatePhase.Completed;
        debate.CompletedAt = DateTimeOffset.UtcNow;
        debate.State = state;

        if (state.Winner != null)
        {
            debate.Result = new DebateResult(
                state.Winner.Member.Slot,
                state.Winner.FinalStatement ?? "Winner determined"
            );
        }

        if (state.Ballots.Length > 0)
        {
            debate.Ballots = state.Ballots
                .Select(b => new BallotItem(
                    b.Slot,
                    b.Ranks.OrderBy(r => r.Rank).Select(r => r.Slot).ToList()
                ))
                .ToList();
        }

        dbContext.Entry(debate).Property(d => d.State).IsModified = true;
        await repository.SaveChangesAsync(cancellationToken);
    }
}
