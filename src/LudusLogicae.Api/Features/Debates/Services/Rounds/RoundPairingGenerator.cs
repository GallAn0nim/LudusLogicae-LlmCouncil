using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Rounds;

public sealed class RoundPairingGenerator
{
    public RoundPairing[] GeneratePairingsForRound(int roundNumber, List<DebateParticipant> participants)
    {
        var slots = participants.Select(p => p.SlotIndex).ToList();
        var offset = CalculateOffset(roundNumber, slots.Count);

        return slots
            .Select(fromSlot => CreatePairing(fromSlot, slots, offset))
            .OrderBy(p => p.FromSlot)
            .ToArray();
    }

    private static int CalculateOffset(int roundNumber, int participantCount)
    {
        return (roundNumber - 1) % participantCount + 1;
    }

    private static RoundPairing CreatePairing(int fromSlot, List<int> slots, int offset)
    {
        var fromIndex = slots.IndexOf(fromSlot);
        var toIndex = (fromIndex + offset) % slots.Count;
        var toSlot = slots[toIndex];

        return new RoundPairing { FromSlot = fromSlot, ToSlot = toSlot };
    }
}
