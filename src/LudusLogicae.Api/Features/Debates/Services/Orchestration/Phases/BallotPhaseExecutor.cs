using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration.Phases;

public record BallotPhaseInput(string Phase, object MembersData);

public sealed class BallotPhaseExecutor(
    ILlmResponseGenerator responseGenerator,
    DebatePromptFactory promptFactory,
    IDebateResponseParser responseParser)
    : BasePhaseExecutor<BallotPhaseInput, MemberBallot[]>(responseGenerator, promptFactory, responseParser)
{
    protected override BallotPhaseInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state)
    {
        return new BallotPhaseInput(
            PhaseNames.Ballot,
            state.BuildMemberDataForBallot());
    }

    protected override int GetMaxTokens()
    {
        return DebateConstants.ShortMaxOutputTokens;
    }

    protected override string GetPhaseDescription()
    {
        return "ballot";
    }

    public override MemberBallot[] ParseResults(List<RawPhaseResult> rawResults)
    {
        return ResponseParser.ParseBallots(rawResults);
    }
}
