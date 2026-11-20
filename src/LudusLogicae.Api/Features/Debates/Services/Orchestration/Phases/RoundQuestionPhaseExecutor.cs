using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration.Phases;

public record RoundQuestionPhaseInput(
    string Phase,
    int TargetSlot,
    string TargetName,
    string TargetCurrentStance,
    string Instruction);

public sealed class RoundQuestionPhaseExecutor(
    ILlmResponseGenerator responseGenerator,
    DebatePromptFactory promptFactory,
    IDebateResponseParser responseParser)
    : BasePhaseExecutor<RoundQuestionPhaseInput, RoundQuestion[]>(responseGenerator, promptFactory, responseParser)
{
    private RoundPairing[] _pairings = [];

    public async Task<List<RawPhaseResult>> ExecutePhaseAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        RoundPairing[] pairings,
        CancellationToken ct)
    {
        _pairings = pairings;
        return await ExecutePhaseAsync(debate, participants, state, ct);
    }

    public RoundQuestion[] ParseResults(List<RawPhaseResult> rawResults, RoundPairing[] pairings)
    {
        _pairings = pairings;
        return ParseResults(rawResults);
    }

    protected override List<PhaseRequest<RoundQuestionPhaseInput>> BuildRequestInputs(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state)
    {
        return participants
            .Select(p =>
            {
                var pairing = _pairings.FirstOrDefault(pair => pair.FromSlot == p.SlotIndex);
                if (pairing == null)
                {
                    return null;
                }

                var targetSlot = pairing.ToSlot;
                var targetName = participants.First(e => e.SlotIndex == targetSlot).Name;

                return new PhaseRequest<RoundQuestionPhaseInput>(
                    p.SlotIndex,
                    p.Provider,
                    p.Model,
                    p.Temperature,
                    GetSystemPromptForParticipant(state, p, debate.NMembers),
                    CreatePhaseInput(debate, p, state, targetSlot, targetName));
            })
            .OfType<PhaseRequest<RoundQuestionPhaseInput>>()
            .ToList();
    }

    private RoundQuestionPhaseInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state,
        int targetSlot,
        string targetName)
    {
        return new RoundQuestionPhaseInput(
            PhaseNames.RoundQuestion,
            targetSlot,
            targetName,
            state.GetLatestStance(targetSlot),
            $"Ask one focused question to {targetName} (Slot {targetSlot}) about their current answer.");
    }

    protected override RoundQuestionPhaseInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state)
    {

        throw new NotImplementedException("Use the overload with pairing information");
    }

    protected override int GetMaxTokens()
    {
        return DebateConstants.ShortMaxOutputTokens;
    }

    protected override string GetPhaseDescription()
    {
        return "round question";
    }

    public override RoundQuestion[] ParseResults(List<RawPhaseResult> rawResults)
    {
        return ResponseParser.ParseRoundQuestions(rawResults, _pairings);
    }
}
