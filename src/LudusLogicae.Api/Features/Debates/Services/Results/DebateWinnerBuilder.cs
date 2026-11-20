using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Results;

public sealed class DebateWinnerBuilder : IDebateWinnerBuilder
{
    public DebateWinner BuildWinner(DebateState state, int winnerSlot)
    {
        var member = GetWinnerMemberInfo(state, winnerSlot);
        var score = GetWinnerScore(state, winnerSlot);
        var finalStatement = GetFinalStatement(state, winnerSlot);
        var initialAnswer = GetInitialAnswer(state, winnerSlot);
        var history = BuildDebateHistory(state, winnerSlot);

        return new DebateWinner
        {
            Member = member,
            TotalPoints = score,
            FinalStatement = finalStatement,
            InitialAnswer = initialAnswer,
            History = history
        };
    }

    private static CouncilMember GetWinnerMemberInfo(DebateState state, int winnerSlot)
    {
        return state.GetMember(winnerSlot) ?? new CouncilMember();
    }

    private static double GetWinnerScore(DebateState state, int winnerSlot)
    {
        return state.Results.FirstOrDefault(r => r.Slot == winnerSlot)?.Score ?? 0;
    }

    private static string GetFinalStatement(DebateState state, int winnerSlot)
    {
        return state.FinalStatements.FirstOrDefault(a => a.Slot == winnerSlot)?.Text ?? string.Empty;
    }

    private static string GetInitialAnswer(DebateState state, int winnerSlot)
    {
        return state.InitialAnswers.FirstOrDefault(a => a.Slot == winnerSlot)?.Text ?? string.Empty;
    }

    private DebateHistoryItem[] BuildDebateHistory(DebateState state, int winnerSlot)
    {
        var history = new List<DebateHistoryItem>();

        foreach (var round in state.Rounds)
        {
            var historyItem = BuildHistoryItemForRound(state, round, winnerSlot);
            history.Add(historyItem);
        }

        return history.ToArray();
    }

    private static DebateHistoryItem BuildHistoryItemForRound(DebateState state, DebateRound round, int winnerSlot)
    {
        var item = new DebateHistoryItem
        {
            RoundNumber = round.RoundNumber,
            Amendment = GetAmendmentForSlot(round, winnerSlot)
        };

        AddQuestionAskedToWinner(state, round, winnerSlot, item);
        AddAnswerGivenByWinner(round, winnerSlot, item);

        return item;
    }

    private static string GetAmendmentForSlot(DebateRound round, int slot)
    {
        return round.Amendments.FirstOrDefault(a => a.Slot == slot)?.Text ?? string.Empty;
    }

    private static void AddQuestionAskedToWinner(DebateState state, DebateRound round, int winnerSlot,
        DebateHistoryItem item)
    {
        var questionToWinner = round.Questions.FirstOrDefault(q => q.ToSlot == winnerSlot);
        if (questionToWinner != null)
        {
            var askerName = state.GetMember(questionToWinner.FromSlot)?.Name ?? "Unknown";
            item.QuestionAsked = $"[From {askerName}] {questionToWinner.Text}";
        }
    }

    private static void AddAnswerGivenByWinner(DebateRound round, int winnerSlot, DebateHistoryItem item)
    {
        var answerGiven = round.Answers.FirstOrDefault(a => a.Slot == winnerSlot);
        if (answerGiven != null)
        {
            item.AnswerGiven = answerGiven.Text;
        }
    }
}
