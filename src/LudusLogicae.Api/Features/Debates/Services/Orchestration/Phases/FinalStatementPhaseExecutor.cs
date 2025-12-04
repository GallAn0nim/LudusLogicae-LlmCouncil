using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration.Phases;

public record FinalStatementPhaseInput(string Phase, string DebateTopic, string Instruction);

public sealed class FinalStatementPhaseExecutor(
    ILlmResponseGenerator responseGenerator,
    DebatePromptFactory promptFactory,
    IDebateResponseParser responseParser)
    : BasePhaseExecutor<FinalStatementPhaseInput, MemberAnswer[]>(responseGenerator, promptFactory, responseParser)
{
    protected override FinalStatementPhaseInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state)
    {
        return new FinalStatementPhaseInput(
            PhaseNames.FinalStatement,
            $"{debate.Question}\n\nContext: {debate.Context}",
            "Produce a single coherent answer, as if you were answering the original question now, with the benefit of the entire discussion.");
    }

    protected override int GetMaxTokens()
    {
        return DebateConstants.StandardMaxOutputTokens;
    }

    protected override string GetPhaseDescription()
    {
        return "final statement";
    }

    public override MemberAnswer[] ParseResults(List<RawPhaseResult> rawResults)
    {
        return ResponseParser.ParseMemberAnswers(rawResults);
    }
}
