using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration.Phases;

public record RoundAnswerPhaseInput(
    string Phase,
    int QuestionFromSlot,
    string QuestionFromName,
    string Question,
    string Instruction);

public sealed class RoundAnswerPhaseExecutor(
    ILlmResponseGenerator responseGenerator,
    DebatePromptFactory promptFactory,
    IDebateResponseParser responseParser)
    : BasePhaseExecutor<RoundAnswerPhaseInput, RoundAnswer[]>(responseGenerator, promptFactory, responseParser)
{
    private RoundQuestion[] _questions = [];

    public async Task<List<RawPhaseResult>> ExecutePhaseAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        RoundQuestion[] questions,
        CancellationToken ct)
    {
        _questions = questions;
        return await ExecutePhaseAsync(debate, participants, state, ct);
    }

    protected override List<PhaseRequest<RoundAnswerPhaseInput>> BuildRequestInputs(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state)
    {
        return participants
            .Select(p =>
            {
                var questionToMe = _questions.FirstOrDefault(q => q.ToSlot == p.SlotIndex);
                if (questionToMe == null)
                {
                    return null;
                }

                var askerName = participants.First(e => e.SlotIndex == questionToMe.FromSlot).Name;

                return new PhaseRequest<RoundAnswerPhaseInput>(
                    p.SlotIndex,
                    p.Provider,
                    p.Model,
                    p.Temperature,
                    GetSystemPromptForParticipant(state, p, debate.NMembers),
                    CreatePhaseInput(debate, p, state, questionToMe, askerName));
            })
            .OfType<PhaseRequest<RoundAnswerPhaseInput>>()
            .ToList();
    }

    private RoundAnswerPhaseInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state,
        RoundQuestion questionToMe,
        string askerName)
    {
        return new RoundAnswerPhaseInput(
            PhaseNames.RoundAnswer,
            questionToMe.FromSlot,
            askerName,
            questionToMe.Text,
            $"Answer the question from {askerName} (Slot {questionToMe.FromSlot}).");
    }

    protected override RoundAnswerPhaseInput CreatePhaseInput(
        Debate debate,
        DebateParticipant participant,
        DebateState state)
    {

        throw new NotImplementedException("Use the overload with question information");
    }

    protected override int GetMaxTokens()
    {
        return DebateConstants.ShortMaxOutputTokens;
    }

    protected override string GetPhaseDescription()
    {
        return "round answer";
    }

    public override RoundAnswer[] ParseResults(List<RawPhaseResult> rawResults)
    {
        return ResponseParser.ParseRoundAnswers(rawResults);
    }
}
