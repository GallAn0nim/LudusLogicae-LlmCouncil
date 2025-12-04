using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration;

public interface IDebateStepExecutor
{
    Task<MemberAnswer[]> ExecuteInitialAnswerRoundAsync(Debate debate, List<DebateParticipant> entries,
        DebateState state, CancellationToken cancellationToken);

    Task<DebateRound> ExecuteRoundAsync(Debate debate, List<DebateParticipant> entries, DebateState state,
        int roundNumber, CancellationToken cancellationToken);

    Task<MemberAnswer[]> ExecuteFinalStatementRoundAsync(Debate debate, List<DebateParticipant> entries,
        DebateState state, CancellationToken cancellationToken);

    Task<MemberBallot[]> ExecuteBallotRoundAsync(Debate debate, List<DebateParticipant> entries, DebateState state,
        CancellationToken cancellationToken);

    Task<MemberVote[]> ExecuteTieBreakRoundAsync(Debate debate, List<DebateParticipant> entries, DebateState state,
        int[] tiedSlots, CancellationToken cancellationToken);
}
