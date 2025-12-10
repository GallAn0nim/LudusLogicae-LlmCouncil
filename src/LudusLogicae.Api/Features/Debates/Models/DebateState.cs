using System.Text.Json.Serialization;

namespace LudusLogicae.Api.Features.Debates.Models;

public sealed class DebateState
{
    public Guid DebateId { get; init; }

    public string Topic { get; init; } = string.Empty;

    public string Context { get; init; } = string.Empty;

    public int MemberCount { get; init; }

    public int TotalRounds { get; init; }

    public CouncilMember[] Members { get; set; } = [];

    [JsonIgnore]
    public Dictionary<int, string> SystemPrompts { get; set; } = new();

    public MemberAnswer[] InitialAnswers { get; set; } = [];

    public DebateRound[] Rounds { get; set; } = [];

    public MemberAnswer[] FinalStatements { get; set; } = [];

    public MemberBallot[] Ballots { get; set; } = [];

    public MemberScore[] Results { get; set; } = [];

    public MemberVote[] TieBreakVotes { get; set; } = [];

    public DebateWinner? Winner { get; set; }

    public string GetLatestStance(int slot)
    {
        var final = FinalStatements.FirstOrDefault(a => a.Slot == slot);
        if (final != null && !string.IsNullOrWhiteSpace(final.Text))
            return final.Text;

        if (Rounds.Length > 0)
        {
            var lastRound = Rounds.Last();
            var amendment = lastRound.Amendments.FirstOrDefault(a => a.Slot == slot);
            if (amendment != null && !string.IsNullOrWhiteSpace(amendment.Text))
                return amendment.Text;
        }

        var initial = InitialAnswers.FirstOrDefault(a => a.Slot == slot);
        return initial?.Text ?? string.Empty;
    }

    public CouncilMember? GetMember(int slot)
    {
        return Members.FirstOrDefault(m => m.Slot == slot);
    }

    public MemberDataForBallot[] BuildMemberDataForBallot()
    {
        return Members
            .OrderBy(m => m.Slot)
            .Select(member => new MemberDataForBallot(
                member.Slot,
                member.Name,
                member.Role,
                GetLatestStance(member.Slot),
                InitialAnswers.FirstOrDefault(a => a.Slot == member.Slot)?.Text ?? ""
            ))
            .ToArray();
    }

    public MemberDataForBallot[] BuildMemberDataForTieBreak(int[] tiedSlots)
    {
        return BuildMemberDataForBallot()
            .Where(m => tiedSlots.Contains(m.Slot))
            .ToArray();
    }
}

public sealed class CouncilMember
{
    public int Slot { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public double Temperature { get; init; }
    public string Introduction { get; init; } = string.Empty;
}

public sealed class DebateWinner
{
    public CouncilMember Member { get; set; } = new();
    public double TotalPoints { get; set; }
    public string FinalStatement { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
    public DebateHistoryItem[] History { get; set; } = [];
}

public sealed class DebateHistoryItem
{
    public int RoundNumber { get; set; }
    public string QuestionAsked { get; set; } = string.Empty;
    public string AnswerGiven { get; set; } = string.Empty;
    public string Amendment { get; set; } = string.Empty;
}

public sealed class MemberAnswer
{
    public int Slot { get; init; }
    public string Text { get; init; } = string.Empty;
}

public sealed class MemberBallot
{
    public int Slot { get; init; }
    public RankEntry[] Ranks { get; init; } = [];
}

public sealed class RankEntry
{
    public int Slot { get; init; }
    public int Rank { get; init; }
}

public sealed class MemberScore
{
    public int Slot { get; init; }
    public double Score { get; set; }
    public int Rank { get; set; }
}

public sealed class MemberVote
{
    public int VoterSlot { get; init; }
    public int VotedForSlot { get; init; }
}
