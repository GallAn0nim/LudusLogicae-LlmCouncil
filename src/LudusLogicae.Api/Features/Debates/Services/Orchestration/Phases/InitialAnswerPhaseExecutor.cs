using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration.Phases;

public record InitialAnswerPhaseInput(string Phase, string DebateTopic);

public sealed class InitialAnswerPhaseExecutor(
    ILlmResponseGenerator responseGenerator,
    DebatePromptFactory promptFactory,
    IDebateResponseParser responseParser)
    : BasePhaseExecutor<InitialAnswerPhaseInput, MemberAnswer[]>(responseGenerator, promptFactory, responseParser)
{
    public async Task<(Dictionary<int, string> SystemPrompts, List<RawPhaseResult> Results)>
        ExecutePhaseWithSystemPromptsAsync(
            Debate debate,
            List<DebateParticipant> participants,
            DebateState state,
            CancellationToken ct)
    {
        var systemPrompts = new Dictionary<int, string>();
        var requestInputs = participants.Select(p =>
        {
            var systemPrompt = PromptFactory.BuildSystemPrompt(p, debate.NMembers);
            systemPrompts[p.SlotIndex] = systemPrompt;

            return (
                p.SlotIndex,
                p.Model,
                p.Temperature,
                SystemPrompt: systemPrompt,
                Input: CreatePhaseInput(debate, p, state)
            );
        }).ToList();

        var result = await ResponseGenerator.GenerateResponsesInParallelAsync(
            requestInputs,
            GetMaxTokens(),
            GetPhaseDescription(),
            ct);

        if (!result.IsFullySuccessful)
        {
            var failureDetails = string.Join("; ", result.Failures.Select(f =>
                $"Slot {f.Slot}: {f.ErrorMessage}"));

            throw new InvalidOperationException(
                $"Failed to generate all {GetPhaseDescription()} responses. " +
                $"{result.SuccessfulResponses.Count}/{result.TotalCount} succeeded. " +
                $"Failures: {failureDetails}");
        }

        return (systemPrompts, result.SuccessfulResponses);
    }

    protected override InitialAnswerPhaseInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state)
    {
        return new InitialAnswerPhaseInput(
            PhaseNames.InitialAnswer,
            $"{debate.Question}\n\nContext: {debate.Context}");
    }

    protected override int GetMaxTokens()
    {
        return DebateConstants.StandardMaxOutputTokens;
    }

    protected override string GetPhaseDescription()
    {
        return "initial answer";
    }

    public override MemberAnswer[] ParseResults(List<RawPhaseResult> rawResults)
    {
        return ResponseParser.ParseMemberAnswers(rawResults);
    }
}
