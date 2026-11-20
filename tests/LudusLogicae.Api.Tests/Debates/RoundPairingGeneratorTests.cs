using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Services.Rounds;
using static LudusLogicae.Api.Tests.TestData;

namespace LudusLogicae.Api.Tests.Debates;

public class RoundPairingGeneratorTests
{
    private readonly RoundPairingGenerator _generator = new();

    public static TheoryData<int> MemberCounts()
    {
        var data = new TheoryData<int>();
        for (var n = DebateConstants.MinimumMemberCount; n <= DebateConstants.MaximumMemberCount; n++)
        {
            data.Add(n);
        }

        return data;
    }

    [Fact]
    public void First_round_pairs_each_member_with_the_next_one_wrapping_around()
    {
        var pairings = _generator.GeneratePairingsForRound(1, Participants(3));

        Assert.Equal([1, 2, 3], pairings.Select(p => p.FromSlot));
        Assert.Equal([2, 3, 1], pairings.Select(p => p.ToSlot));
    }

    [Theory]
    [MemberData(nameof(MemberCounts))]
    public void Every_member_asks_once_and_is_asked_once_per_round(int members)
    {
        for (var round = 1; round < members; round++)
        {
            var pairings = _generator.GeneratePairingsForRound(round, Participants(members));

            Assert.Equal(Enumerable.Range(1, members), pairings.Select(p => p.FromSlot));
            Assert.Equal(Enumerable.Range(1, members), pairings.Select(p => p.ToSlot).Order());
        }
    }

    [Theory]
    [MemberData(nameof(MemberCounts))]
    public void No_member_questions_themselves_in_the_first_n_minus_1_rounds(int members)
    {
        for (var round = 1; round < members; round++)
        {
            Assert.All(_generator.GeneratePairingsForRound(round, Participants(members)),
                p => Assert.NotEqual(p.FromSlot, p.ToSlot));
        }
    }

    [Theory]
    [MemberData(nameof(MemberCounts))]
    public void Pairings_never_repeat_in_the_first_n_minus_1_rounds(int members)
    {
        var all = Enumerable.Range(1, members - 1)
            .SelectMany(round => _generator.GeneratePairingsForRound(round, Participants(members)))
            .ToList();

        Assert.All(all.GroupBy(p => p.FromSlot), asked => Assert.Distinct(asked.Select(p => p.ToSlot)));
        Assert.Equal(members * (members - 1), all.Count);
    }

    [Theory]
    [MemberData(nameof(MemberCounts))]
    public void After_n_minus_1_rounds_pairings_cycle_back_to_round_one(int members)
    {
        var first = _generator.GeneratePairingsForRound(1, Participants(members));
        var wrapped = _generator.GeneratePairingsForRound(members, Participants(members));

        Assert.Equal(first.Select(p => p.FromSlot), wrapped.Select(p => p.FromSlot));
        Assert.Equal(first.Select(p => p.ToSlot), wrapped.Select(p => p.ToSlot));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Fewer_than_two_participants_gives_no_pairings(int members)
    {
        Assert.Empty(_generator.GeneratePairingsForRound(1, Participants(members)));
    }

    [Fact]
    public void Works_with_non_contiguous_and_unordered_slots()
    {
        var pairings = _generator.GeneratePairingsForRound(1, Participants(5, 1, 3));

        Assert.Equal([1, 3, 5], pairings.Select(p => p.FromSlot));
        Assert.All(pairings, p => Assert.NotEqual(p.FromSlot, p.ToSlot));
        Assert.Equal([1, 3, 5], pairings.Select(p => p.ToSlot).Order());
    }

    [Theory]
    [MemberData(nameof(MemberCounts))]
    public void No_member_questions_themselves_in_any_round(int members)
    {
        for (var round = 1; round <= members * 2; round++)
        {
            Assert.All(_generator.GeneratePairingsForRound(round, Participants(members)),
                p => Assert.NotEqual(p.FromSlot, p.ToSlot));
        }
    }
}
