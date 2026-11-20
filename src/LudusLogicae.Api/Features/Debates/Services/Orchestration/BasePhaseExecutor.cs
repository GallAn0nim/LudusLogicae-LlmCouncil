using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration;

public abstract class BasePhaseExecutor<TInput, TOutput>(
    ILlmResponseGenerator responseGenerator,
    DebatePromptFactory promptFactory,
    IDebateResponseParser responseParser)
    : IPhaseExecutor<TInput, TOutput>
{
    protected readonly DebatePromptFactory PromptFactory = promptFactory;
    protected readonly ILlmResponseGenerator ResponseGenerator = responseGenerator;
    protected readonly IDebateResponseParser ResponseParser = responseParser;

    public virtual async Task<List<RawPhaseResult>> ExecutePhaseAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken ct)
    {
        var requestInputs = BuildRequestInputs(debate, participants, state);

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

        return result.SuccessfulResponses;
    }

    public abstract TOutput ParseResults(List<RawPhaseResult> rawResults);

    protected virtual List<PhaseRequest<TInput>> BuildRequestInputs(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state)
    {
        return participants.Select(p => new PhaseRequest<TInput>(
            p.SlotIndex,
            p.Provider,
            p.Model,
            p.Temperature,
            GetSystemPromptForParticipant(state, p, debate.NMembers),
            CreatePhaseInput(debate, p, state))).ToList();
    }

    protected string GetSystemPromptForParticipant(DebateState state, DebateParticipant participant, int memberCount)
    {
        return state.SystemPrompts.TryGetValue(participant.SlotIndex, out var prompt)
            ? prompt
            : PromptFactory.BuildSystemPrompt(participant, memberCount);
    }

    protected abstract TInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state);

    protected abstract int GetMaxTokens();

    protected abstract string GetPhaseDescription();
}
