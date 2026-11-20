using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Features.Debates.Services.Results;
using Microsoft.Extensions.Logging.Abstractions;
using static LudusLogicae.Api.Tests.TestData;

namespace LudusLogicae.Api.Tests.Debates;

public class DebateResponseParserTests
{
    private readonly DebateResponseParser _parser = new(NullLogger<DebateResponseParser>.Instance);

    [Fact]
    public void Text_answers_keep_raw_text_and_are_ordered_by_slot()
    {
        var answers = _parser.ParseMemberAnswers([new(3, "C"), new(1, "A"), new(2, "B")]);

        Assert.Equal([1, 2, 3], answers.Select(a => a.Slot));
        Assert.Equal(["A", "B", "C"], answers.Select(a => a.Text));
    }

    [Fact]
    public void Round_answers_and_amendments_map_slot_and_text()
    {
        var answers = _parser.ParseRoundAnswers([new(2, "ans")]);
        var amendments = _parser.ParseRoundAmendments([new(2, "amend")]);

        Assert.Equal(2, answers.Single().Slot);
        Assert.Equal("ans", answers.Single().Text);
        Assert.Equal(2, amendments.Single().Slot);
        Assert.Equal("amend", amendments.Single().Text);
    }

    [Fact]
    public void Questions_get_target_from_pairing_and_unpaired_results_are_dropped()
    {
        RoundPairing[] pairings = [new() { FromSlot = 1, ToSlot = 2 }, new() { FromSlot = 2, ToSlot = 1 }];

        var questions = _parser.ParseRoundQuestions([new(1, "Q1"), new(2, "Q2"), new(9, "stray")], pairings);

        Assert.Equal([1, 2], questions.Select(q => q.FromSlot));
        Assert.Equal([2, 1], questions.Select(q => q.ToSlot));
        Assert.Equal(["Q1", "Q2"], questions.Select(q => q.Text));
    }

    [Fact]
    public void Ballot_json_is_parsed_and_ranks_sorted_by_rank()
    {
        var ballot = _parser.ParseBallots([new(1, """{"ranks":[{"slot":3,"rank":2},{"slot":2,"rank":1},{"slot":1,"rank":3}]}""")]).Single();

        Assert.Equal(1, ballot.Slot);
        Assert.Equal([2, 3, 1], ballot.Ranks.Select(r => r.Slot));
        Assert.Equal([1, 2, 3], ballot.Ranks.Select(r => r.Rank));
    }

    [Theory]
    [InlineData("```json\n{\"ranks\":[{\"slot\":2,\"rank\":1},{\"slot\":1,\"rank\":2}]}\n```")]
    [InlineData("Here is my ballot: {\"ranks\":[{\"slot\":2,\"rank\":1},{\"slot\":1,\"rank\":2}]} Thanks!")]
    public void Ballot_json_wrapped_in_prose_or_code_fence_is_still_parsed(string text)
    {
        var ballot = _parser.ParseBallots([new(3, text)]).Single();

        Assert.Equal([2, 1], ballot.Ranks.Select(r => r.Slot));
        Assert.Equal([1, 2], ballot.Ranks.Select(r => r.Rank));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("I refuse to vote.")]
    [InlineData("{\"ranks\": [ {\"slot\": 1, ")]
    [InlineData("{\"something\":\"else\"}")]
    [InlineData("{\"ranks\":null}")]
    public void Malformed_ballot_becomes_empty_ballot_without_throwing(string text)
    {
        var ballot = _parser.ParseBallots([new(2, text)]).Single();

        Assert.Equal(2, ballot.Slot);
        Assert.Empty(ballot.Ranks);
    }

    [Fact]
    public void One_malformed_ballot_does_not_affect_the_others()
    {
        var ballots = _parser.ParseBallots([
            new(1, """{"ranks":[{"slot":2,"rank":1},{"slot":3,"rank":2},{"slot":1,"rank":3}]}"""),
            new(2, "garbage"),
            new(3, """{"ranks":[{"slot":2,"rank":1},{"slot":1,"rank":2},{"slot":3,"rank":3}]}""")]);

        Assert.Equal([3, 0, 3], ballots.Select(b => b.Ranks.Length));
    }

    [Fact]
    public void Malformed_ballot_contributes_no_points_to_the_result()
    {
        var ballots = _parser.ParseBallots([
            new(1, """{"ranks":[{"slot":2,"rank":1},{"slot":3,"rank":2},{"slot":1,"rank":3}]}"""),
            new(2, "not json")]);

        var results = new DebateResultCalculator().CalculateMemberScores(Debate(3), new DebateState { Ballots = ballots });

        Assert.Equal([1.0, 3.0, 2.0], results.OrderBy(r => r.Slot).Select(r => r.Score));
    }

    [Fact]
    public void Tie_break_vote_json_is_parsed()
    {
        var vote = _parser.ParseTieBreakVotes([new(4, "My vote: {\"vote\": 2}")]).Single();

        Assert.Equal(4, vote.VoterSlot);
        Assert.Equal(2, vote.VotedForSlot);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Slot 2 obviously")]
    [InlineData("{\"vote\":\"two\"}")]
    public void Malformed_tie_break_vote_becomes_vote_for_slot_zero(string text)
    {
        var vote = _parser.ParseTieBreakVotes([new(4, text)]).Single();

        Assert.Equal(4, vote.VoterSlot);
        Assert.Equal(0, vote.VotedForSlot);
    }
}
