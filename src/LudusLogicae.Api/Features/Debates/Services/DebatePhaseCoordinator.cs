using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Orchestration;
using LudusLogicae.Api.Features.Debates.Services.Results;

namespace LudusLogicae.Api.Features.Debates.Services;

public sealed class DebatePhaseCoordinator(
    IDebateStepExecutor stepExecutor,
    IDebateResultCalculator resultCalculator,
    IDebateWinnerBuilder winnerBuilder) : IDebatePhaseCoordinator
{
    public async Task ExecuteAllPhasesAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken)
    {
        await ExecuteInitialAnswerPhaseAsync(debate, participants, state, cancellationToken);
        await ExecuteRoundPhasesAsync(debate, participants, state, cancellationToken);
        await ExecuteFinalPhaseAsync(debate, participants, state, cancellationToken);
        await HandleTieBreakIfNeededAsync(debate, participants, state, cancellationToken);
        DetermineWinnerAsync(state);
    }

    private async Task ExecuteInitialAnswerPhaseAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken)
    {
        state.InitialAnswers = await stepExecutor.ExecuteInitialAnswerRoundAsync(
            debate,
            participants,
            state,
            cancellationToken);
        debate.Phase = DebatePhase.CrossQuestion;
    }

    private async Task ExecuteRoundPhasesAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken)
    {
        var totalRounds = state.TotalRounds > 0 ? state.TotalRounds : 3;
        for (var i = 1; i <= totalRounds; i++)
        {
            await stepExecutor.ExecuteRoundAsync(debate, participants, state, i, cancellationToken);
        }
    }

    private async Task ExecuteFinalPhaseAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken)
    {
        state.FinalStatements = await stepExecutor.ExecuteFinalStatementRoundAsync(
            debate,
            participants,
            state,
            cancellationToken);
        debate.Phase = DebatePhase.FinalAnswer;

        state.Ballots = await stepExecutor.ExecuteBallotRoundAsync(debate, participants, state, cancellationToken);
        debate.Phase = DebatePhase.Ballot;

        state.Results = resultCalculator.CalculateMemberScores(debate, state);
    }

    private async Task HandleTieBreakIfNeededAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken)
    {
        if (!resultCalculator.HasTieForFirstPlace(state, out var tiedSlots))
        {
            return;
        }

        state.TieBreakVotes = await stepExecutor.ExecuteTieBreakRoundAsync(
            debate,
            participants,
            state,
            tiedSlots,
            cancellationToken);

        var winnerSlot = resultCalculator.DetermineWinnerFromTieBreakVotes(tiedSlots, state.TieBreakVotes);
        resultCalculator.ApplyTieBreakBonus(state, winnerSlot);
        state.Winner = winnerBuilder.BuildWinner(state, winnerSlot);
    }

    private void DetermineWinnerAsync(DebateState state)
    {
        if (state.Winner == null && state.Results.Length > 0)
        {
            state.Winner = winnerBuilder.BuildWinner(state, state.Results[0].Slot);
        }
    }
}
