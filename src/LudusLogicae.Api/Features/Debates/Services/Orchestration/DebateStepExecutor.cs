using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Orchestration.Phases;
using LudusLogicae.Api.Features.Debates.Services.Rounds;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration;

public sealed class DebateStepExecutor(
    InitialAnswerPhaseExecutor initialAnswerExecutor,
    RoundQuestionPhaseExecutor roundQuestionExecutor,
    RoundAnswerPhaseExecutor roundAnswerExecutor,
    AmendmentPhaseExecutor amendmentExecutor,
    FinalStatementPhaseExecutor finalStatementExecutor,
    BallotPhaseExecutor ballotExecutor,
    TieBreakPhaseExecutor tieBreakExecutor,
    RoundPairingGenerator pairingGenerator)
    : IDebateStepExecutor
{
    public async Task<MemberAnswer[]> ExecuteInitialAnswerRoundAsync(
        Debate debate,
        List<DebateParticipant> entries,
        DebateState state,
        CancellationToken cancellationToken)
    {
        var result = await initialAnswerExecutor.ExecutePhaseWithSystemPromptsAsync(
            debate, entries, state, cancellationToken);
        state.SystemPrompts = result.SystemPrompts;

        return initialAnswerExecutor.ParseResults(result.Results);
    }

    public async Task<DebateRound> ExecuteRoundAsync(
        Debate debate,
        List<DebateParticipant> entries,
        DebateState state,
        int roundNumber,
        CancellationToken cancellationToken)
    {
        var round = new DebateRound
        {
            RoundNumber = roundNumber,
            Pairings = pairingGenerator.GeneratePairingsForRound(roundNumber, entries)
        };

        var rawQuestions = await roundQuestionExecutor.ExecutePhaseAsync(
            debate, entries, state, round.Pairings, cancellationToken);
        round.Questions = roundQuestionExecutor.ParseResults(rawQuestions, round.Pairings);

        var rawAnswers = await roundAnswerExecutor.ExecutePhaseAsync(
            debate, entries, state, round.Questions, cancellationToken);
        round.Answers = roundAnswerExecutor.ParseResults(rawAnswers);

        var currentRounds = state.Rounds.ToList();
        currentRounds.Add(round);
        state.Rounds = currentRounds.ToArray();

        var rawAmendments = await amendmentExecutor.ExecutePhaseAsync(
            debate, entries, state, roundNumber, cancellationToken);
        round.Amendments = amendmentExecutor.ParseResults(rawAmendments);

        return round;
    }

    public async Task<MemberAnswer[]> ExecuteFinalStatementRoundAsync(
        Debate debate,
        List<DebateParticipant> entries,
        DebateState state,
        CancellationToken cancellationToken)
    {
        var rawResults = await finalStatementExecutor.ExecutePhaseAsync(debate, entries, state, cancellationToken);
        return finalStatementExecutor.ParseResults(rawResults);
    }

    public async Task<MemberBallot[]> ExecuteBallotRoundAsync(
        Debate debate,
        List<DebateParticipant> entries,
        DebateState state,
        CancellationToken cancellationToken)
    {
        var rawResults = await ballotExecutor.ExecutePhaseAsync(debate, entries, state, cancellationToken);
        return ballotExecutor.ParseResults(rawResults);
    }

    public async Task<MemberVote[]> ExecuteTieBreakRoundAsync(
        Debate debate,
        List<DebateParticipant> entries,
        DebateState state,
        int[] tiedSlots,
        CancellationToken cancellationToken)
    {
        var rawResults =
            await tieBreakExecutor.ExecutePhaseAsync(debate, entries, state, tiedSlots, cancellationToken);
        return tieBreakExecutor.ParseResults(rawResults);
    }
}
