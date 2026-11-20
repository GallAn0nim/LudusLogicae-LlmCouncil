using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Results;
using static LudusLogicae.Api.Tests.TestData;

namespace LudusLogicae.Api.Tests.Debates;

public class DebateResultCalculatorTests
{
    private readonly DebateResultCalculator _calculator = new();

    private MemberScore[] Calculate(int members, params MemberBallot[] ballots)
    {
        return _calculator.CalculateMemberScores(Debate(members), new DebateState { Ballots = ballots });
    }

    private static double ScoreOf(MemberScore[] results, int slot)
    {
        return results.Single(r => r.Slot == slot).Score;
    }

    [Fact]
    public void Borda_count_gives_n_minus_rank_plus_one_points_and_orders_by_score()
    {
        var results = Calculate(3,
            Ballot(1, 2, 3, 1),
            Ballot(2, 2, 1, 3),
            Ballot(3, 1, 2, 3));

        Assert.Equal([2, 1, 3], results.Select(r => r.Slot));
        Assert.Equal([8.0, 6.0, 4.0], results.Select(r => r.Score));
        Assert.Equal([1, 2, 3], results.Select(r => r.Rank));
    }

    [Fact]
    public void Every_member_gets_a_result_even_without_ballots()
    {
        var results = Calculate(5);

        Assert.Equal(5, results.Length);
        Assert.All(results, r => Assert.Equal(0, r.Score));
        Assert.Equal([1, 2, 3, 4, 5], results.Select(r => r.Slot).Order());
    }

    [Fact]
    public void Total_points_equal_ballots_times_triangular_number()
    {
        var results = Calculate(5,
            Ballot(1, 2, 3, 4, 5, 1),
            Ballot(2, 1, 3, 4, 5, 2),
            Ballot(3, 5, 4, 2, 1, 3));

        Assert.Equal(3 * 15, results.Sum(r => r.Score));
    }

    [Fact]
    public void Ballot_with_slot_above_member_count_is_ignored()
    {
        var results = Calculate(3, Ballot(1, 4, 2, 3), Ballot(2, 1, 2, 3));

        Assert.Equal(3, ScoreOf(results, 1));
        Assert.DoesNotContain(results, r => r.Slot == 4);
    }

    [Fact]
    public void Ballot_with_slot_zero_or_negative_is_ignored()
    {
        var results = Calculate(3, Ballot(1, 0, 2, 3), Ballot(2, -1, 2, 3), Ballot(3, 1, 2, 3));

        Assert.Equal([3.0, 2.0, 1.0], results.OrderBy(r => r.Slot).Select(r => r.Score));
    }

    [Fact]
    public void Ballot_ranking_the_same_slot_twice_is_ignored()
    {
        var results = Calculate(3, Ballot(1, 2, 2, 3), Ballot(2, 1, 2, 3));

        Assert.Equal([3.0, 2.0, 1.0], results.OrderBy(r => r.Slot).Select(r => r.Score));
    }

    [Fact]
    public void Empty_ballot_is_ignored()
    {
        var results = Calculate(3, new MemberBallot { Slot = 1, Ranks = [] }, Ballot(2, 1, 2, 3));

        Assert.Equal([3.0, 2.0, 1.0], results.OrderBy(r => r.Slot).Select(r => r.Score));
    }

    [Fact]
    public void Partial_ballot_with_valid_slots_still_counts()
    {
        var results = Calculate(3, Ballot(1, 2));

        Assert.Equal(3, ScoreOf(results, 2));
        Assert.Equal(0, ScoreOf(results, 1));
    }

    [Fact]
    public void Ballot_with_rank_zero_is_ignored()
    {
        var results = Calculate(3, RawBallot(1, 0, 1, 2));

        Assert.All(results, r => Assert.Equal(0, r.Score));
    }

    [Fact]
    public void Ballot_with_rank_above_member_count_is_ignored()
    {
        var results = Calculate(3, RawBallot(1, 1, 2, 5));

        Assert.All(results, r => Assert.Equal(0, r.Score));
    }

    [Fact]
    public void Ballot_with_duplicate_ranks_is_ignored()
    {
        var results = Calculate(3, RawBallot(1, 1, 1, 2));

        Assert.All(results, r => Assert.Equal(0, r.Score));
    }

    [Fact]
    public void Detects_tie_for_first_place()
    {
        var state = new DebateState { Results = [Score(1, 5), Score(2, 5), Score(3, 2)] };

        Assert.True(_calculator.HasTieForFirstPlace(state, out var tied));
        Assert.Equal([1, 2], tied);
    }

    [Fact]
    public void No_tie_when_single_leader()
    {
        var state = new DebateState { Results = [Score(2, 7), Score(1, 5), Score(3, 5)] };

        Assert.False(_calculator.HasTieForFirstPlace(state, out var tied));
        Assert.Equal([2], tied);
    }

    [Fact]
    public void No_tie_when_no_results()
    {
        Assert.False(_calculator.HasTieForFirstPlace(new DebateState(), out var tied));
        Assert.Empty(tied);
    }

    [Fact]
    public void Tie_break_winner_is_slot_with_most_votes()
    {
        var winner = _calculator.DetermineWinnerFromTieBreakVotes([1, 2],
        [
            new MemberVote { VoterSlot = 3, VotedForSlot = 2 },
            new MemberVote { VoterSlot = 4, VotedForSlot = 2 },
            new MemberVote { VoterSlot = 5, VotedForSlot = 1 }
        ]);

        Assert.Equal(2, winner);
    }

    [Fact]
    public void Tie_break_votes_for_slots_outside_the_tie_are_ignored()
    {
        var winner = _calculator.DetermineWinnerFromTieBreakVotes([1, 2],
        [
            new MemberVote { VoterSlot = 3, VotedForSlot = 3 },
            new MemberVote { VoterSlot = 4, VotedForSlot = 3 },
            new MemberVote { VoterSlot = 5, VotedForSlot = 0 },
            new MemberVote { VoterSlot = 2, VotedForSlot = 1 }
        ]);

        Assert.Equal(1, winner);
    }

    [Fact]
    public void Undecided_tie_break_falls_back_to_first_tied_slot()
    {
        Assert.Equal(3, _calculator.DetermineWinnerFromTieBreakVotes([3, 1], []));
        Assert.Equal(3, _calculator.DetermineWinnerFromTieBreakVotes([3, 1],
        [
            new MemberVote { VoterSlot = 2, VotedForSlot = 1 },
            new MemberVote { VoterSlot = 4, VotedForSlot = 3 }
        ]));
    }

    [Fact]
    public void Tie_break_bonus_moves_winner_to_first_and_reranks()
    {
        var state = new DebateState { Results = [Score(1, 5, 1), Score(2, 5, 2), Score(3, 2, 3)] };

        _calculator.ApplyTieBreakBonus(state, 2);

        Assert.Equal([2, 1, 3], state.Results.Select(r => r.Slot));
        Assert.Equal(5 + DebateConstants.TieBreakBonusPoints, state.Results[0].Score);
        Assert.Equal([1, 2, 3], state.Results.Select(r => r.Rank));
    }

    [Fact]
    public void Tie_break_bonus_for_unknown_slot_changes_no_scores()
    {
        var state = new DebateState { Results = [Score(1, 5, 1), Score(2, 3, 2)] };

        _calculator.ApplyTieBreakBonus(state, 9);

        Assert.Equal([5.0, 3.0], state.Results.Select(r => r.Score));
    }
}
