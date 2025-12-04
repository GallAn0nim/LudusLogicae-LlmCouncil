using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Results;

public sealed class DebateResultCalculator : IDebateResultCalculator
{
    public MemberScore[] CalculateMemberScores(Debate debate, DebateState state)
    {
        var scores = InitializeScoreDictionary(debate.NMembers);
        var validBallots = state.Ballots.Where(ballot => IsBallotValid(ballot, debate.NMembers)).ToArray();
        ApplyBallotScores(validBallots, debate.NMembers, scores);
        var results = ConvertScoresToResults(scores);
        AssignRanks(results);

        return results.OrderBy(r => r.Rank).ToArray();
    }

    public void ApplyTieBreakBonus(DebateState state, int winnerSlot)
    {
        var results = state.Results.ToList();
        var winner = results.FirstOrDefault(r => r.Slot == winnerSlot);

        if (winner != null) winner.Score += DebateConstants.TieBreakBonusPoints;

        state.Results = results.OrderByDescending(r => r.Score).ToArray();
        AssignRanks(state.Results);
    }

    public int DetermineWinnerFromTieBreakVotes(int[] tiedSlots, MemberVote[] tieBreakVotes)
    {
        var voteCounts = InitializeVoteCounts(tiedSlots);
        CountVotes(tieBreakVotes, voteCounts);

        return voteCounts.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public bool HasTieForFirstPlace(DebateState state, out int[] tiedSlots)
    {
        var topScore = state.Results.FirstOrDefault()?.Score ?? 0;
        var tiedForFirst = state.Results.Where(r => r.Score == topScore).ToArray();

        tiedSlots = tiedForFirst.Select(r => r.Slot).ToArray();
        return tiedSlots.Length > 1;
    }

    private static Dictionary<int, double> InitializeScoreDictionary(int memberCount)
    {
        return Enumerable.Range(1, memberCount)
            .ToDictionary(slot => slot, _ => 0.0);
    }

    private static void ApplyBallotScores(MemberBallot[] ballots, int memberCount, Dictionary<int, double> scores)
    {
        foreach (var ballot in ballots)
            foreach (var rank in ballot.Ranks)
            {
                var points = memberCount - rank.Rank + 1;
                scores[rank.Slot] = scores.GetValueOrDefault(rank.Slot, 0) + points;
            }
    }

    private static List<MemberScore> ConvertScoresToResults(Dictionary<int, double> scores)
    {
        return scores
            .Select(kvp => new MemberScore { Slot = kvp.Key, Score = kvp.Value })
            .OrderByDescending(r => r.Score)
            .ToList();
    }

    private static void AssignRanks(IList<MemberScore> results)
    {
        for (var i = 0; i < results.Count; i++) results[i].Rank = i + 1;
    }

    private static Dictionary<int, int> InitializeVoteCounts(int[] tiedSlots)
    {
        return tiedSlots.ToDictionary(slot => slot, _ => 0);
    }

    private static void CountVotes(MemberVote[] votes, Dictionary<int, int> voteCounts)
    {
        foreach (var vote in votes)
            if (voteCounts.ContainsKey(vote.VotedForSlot))
                voteCounts[vote.VotedForSlot]++;
    }

    private static bool IsBallotValid(MemberBallot ballot, int memberCount)
    {
        if (ballot.Ranks == null || ballot.Ranks.Length == 0) return false;

        var slots = ballot.Ranks.Select(r => r.Slot).ToArray();

        foreach (var slot in slots)
            if (slot < 1 || slot > memberCount)
                return false;

        if (slots.Length != slots.Distinct().Count()) return false;

        return true;
    }
}
