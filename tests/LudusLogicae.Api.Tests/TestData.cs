using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Tests;

internal static class TestData
{
    public static Debate Debate(int members)
    {
        return new() { NMembers = members };
    }

    public static MemberBallot Ballot(int voterSlot, params int[] slotsInRankOrder)
    {
        return new()
        {
            Slot = voterSlot,
            Ranks = slotsInRankOrder.Select((slot, i) => new RankEntry { Slot = slot, Rank = i + 1 }).ToArray()
        };
    }

    public static MemberBallot RawBallot(int voterSlot, params int[] ranksInSlotOrder)
    {
        return new()
        {
            Slot = voterSlot,
            Ranks = ranksInSlotOrder.Select((rank, i) => new RankEntry { Slot = i + 1, Rank = rank }).ToArray()
        };
    }

    public static MemberScore Score(int slot, double score, int rank = 0)
    {
        return new() { Slot = slot, Score = score, Rank = rank };
    }

    public static List<DebateParticipant> Participants(params int[] slots)
    {
        return slots.Select(s => new DebateParticipant { SlotIndex = s }).ToList();
    }

    public static List<DebateParticipant> Participants(int count)
    {
        return Participants(Enumerable.Range(1, count).ToArray());
    }
}
