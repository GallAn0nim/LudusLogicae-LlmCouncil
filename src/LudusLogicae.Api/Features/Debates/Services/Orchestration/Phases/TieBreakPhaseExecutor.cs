using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration.Phases;

public record TieBreakPhaseInput(string Phase, int[] TiedSlots, object MembersData);

public sealed class TieBreakPhaseExecutor(
    ILlmResponseGenerator responseGenerator,
    DebatePromptFactory promptFactory,
    IDebateResponseParser responseParser)
    : BasePhaseExecutor<TieBreakPhaseInput, MemberVote[]>(responseGenerator, promptFactory, responseParser)
{
    private int[] _tiedSlots = [];

    public async Task<List<RawPhaseResult>> ExecutePhaseAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        int[] tiedSlots,
        CancellationToken ct)
    {
        _tiedSlots = tiedSlots;
        return await ExecutePhaseAsync(debate, participants, state, ct);
    }

    protected override TieBreakPhaseInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state)
    {
        return new TieBreakPhaseInput(
            "Tie-Break Vote",
            _tiedSlots,
            state.BuildMemberDataForTieBreak(_tiedSlots));
    }

    protected override int GetMaxTokens()
    {
        return DebateConstants.TieBreakMaxOutputTokens;
    }

    protected override string GetPhaseDescription()
    {
        return "tie-break";
    }

    public override MemberVote[] ParseResults(List<RawPhaseResult> rawResults)
    {
        return ResponseParser.ParseTieBreakVotes(rawResults);
    }
}
