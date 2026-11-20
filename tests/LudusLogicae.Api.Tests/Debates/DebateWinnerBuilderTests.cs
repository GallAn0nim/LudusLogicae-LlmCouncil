using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Results;
using static LudusLogicae.Api.Tests.TestData;

namespace LudusLogicae.Api.Tests.Debates;

public class DebateWinnerBuilderTests
{
    private readonly DebateWinnerBuilder _builder = new();

    private static DebateState CompletedState()
    {
        return new()
        {
            Members =
        [
            new CouncilMember { Slot = 1, Name = "Alice", Role = "Philosopher" },
            new CouncilMember { Slot = 2, Name = "Bob", Role = "Scientist" }
        ],
            InitialAnswers = [new MemberAnswer { Slot = 1, Text = "A0" }, new MemberAnswer { Slot = 2, Text = "B0" }],
            FinalStatements = [new MemberAnswer { Slot = 1, Text = "A-final" }, new MemberAnswer { Slot = 2, Text = "B-final" }],
            Results = [Score(2, 8, 1), Score(1, 6, 2)],
            Rounds =
        [
            new DebateRound
            {
                RoundNumber = 1,
                Questions =
                [
                    new RoundQuestion { FromSlot = 1, ToSlot = 2, Text = "Why?" },
                    new RoundQuestion { FromSlot = 2, ToSlot = 1, Text = "How?" }
                ],
                Answers = [new RoundAnswer { Slot = 2, Text = "Because." }, new RoundAnswer { Slot = 1, Text = "So." }],
                Amendments = [new RoundAmendment { Slot = 2, Text = "B-amended" }]
            },
            new DebateRound { RoundNumber = 2 }
        ]
        };
    }

    [Fact]
    public void Builds_winner_from_their_own_data_only()
    {
        var winner = _builder.BuildWinner(CompletedState(), 2);

        Assert.Equal("Bob", winner.Member.Name);
        Assert.Equal(8, winner.TotalPoints);
        Assert.Equal("B0", winner.InitialAnswer);
        Assert.Equal("B-final", winner.FinalStatement);
    }

    [Fact]
    public void History_has_one_item_per_round_with_question_to_winner_and_their_answer()
    {
        var history = _builder.BuildWinner(CompletedState(), 2).History;

        Assert.Equal([1, 2], history.Select(h => h.RoundNumber));
        Assert.Equal("[From Alice] Why?", history[0].QuestionAsked);
        Assert.Equal("Because.", history[0].AnswerGiven);
        Assert.Equal("B-amended", history[0].Amendment);
        Assert.Equal(string.Empty, history[1].QuestionAsked);
        Assert.Equal(string.Empty, history[1].AnswerGiven);
        Assert.Equal(string.Empty, history[1].Amendment);
    }

    [Fact]
    public void Unknown_asker_is_labelled_unknown()
    {
        var state = CompletedState();
        state.Members = state.Members.Where(m => m.Slot != 1).ToArray();

        var history = _builder.BuildWinner(state, 2).History;

        Assert.Equal("[From Unknown] Why?", history[0].QuestionAsked);
    }

    [Fact]
    public void Missing_winner_data_gives_empty_defaults_instead_of_throwing()
    {
        var winner = _builder.BuildWinner(new DebateState(), 3);

        Assert.Equal(string.Empty, winner.Member.Name);
        Assert.Equal(0, winner.TotalPoints);
        Assert.Equal(string.Empty, winner.FinalStatement);
        Assert.Equal(string.Empty, winner.InitialAnswer);
        Assert.Empty(winner.History);
    }
}
