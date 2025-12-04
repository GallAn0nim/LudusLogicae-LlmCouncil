using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Results;

public interface IDebateResultCalculator
{
    MemberScore[] CalculateMemberScores(Debate debate, DebateState state);

    void ApplyTieBreakBonus(DebateState state, int winnerSlot);

    int DetermineWinnerFromTieBreakVotes(int[] tiedSlots, MemberVote[] tieBreakVotes);

    bool HasTieForFirstPlace(DebateState state, out int[] tiedSlots);
}
