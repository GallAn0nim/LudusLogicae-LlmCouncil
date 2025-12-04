using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration;

public interface IPhaseExecutor<TInput, TOutput>
{
    Task<List<RawPhaseResult>> ExecutePhaseAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken ct);

    TOutput ParseResults(List<RawPhaseResult> rawResults);
}
