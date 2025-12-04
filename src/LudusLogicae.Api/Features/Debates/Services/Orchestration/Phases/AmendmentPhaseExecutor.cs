using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Features.Debates.Services.Rounds;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration.Phases;

public record AmendmentPhaseInput(string Phase, string RoundTranscript, string Instruction);

public sealed class AmendmentPhaseExecutor(
    ILlmResponseGenerator responseGenerator,
    DebatePromptFactory promptFactory,
    IDebateResponseParser responseParser,
    RoundTranscriptFormatter transcriptFormatter)
    : BasePhaseExecutor<AmendmentPhaseInput, RoundAmendment[]>(responseGenerator, promptFactory, responseParser)
{
    private List<DebateParticipant> _participants = [];
    private int _roundNumber;

    public async Task<List<RawPhaseResult>> ExecutePhaseAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        int roundNumber,
        CancellationToken ct)
    {
        _roundNumber = roundNumber;
        _participants = participants;
        return await ExecutePhaseAsync(debate, participants, state, ct);
    }

    protected override AmendmentPhaseInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state)
    {
        var currentRound = state.Rounds.FirstOrDefault(r => r.RoundNumber == _roundNumber);
        var roundTranscript = currentRound != null
            ? transcriptFormatter.FormatRoundTranscript(currentRound, _participants)
            : string.Empty;

        return new AmendmentPhaseInput(
            "Round Amendment/Synthesis",
            roundTranscript,
            "Refine your stance based on the round's discussion.");
    }

    protected override int GetMaxTokens()
    {
        return DebateConstants.StandardMaxOutputTokens;
    }

    protected override string GetPhaseDescription()
    {
        return "round amendment";
    }

    public override RoundAmendment[] ParseResults(List<RawPhaseResult> rawResults)
    {
        return ResponseParser.ParseRoundAmendments(rawResults);
    }
}
