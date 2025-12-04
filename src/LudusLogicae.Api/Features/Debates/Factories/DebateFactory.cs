using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Factories;

public sealed class DebateFactory : IDebateFactory
{
    public Debate CreateDebateEntity(CreateDebateRequest request)
    {
        return new Debate
        {
            Question = request.Question,
            NMembers = request.NMembers,
            NRounds = request.NRounds,
            Context = request.Context ?? string.Empty,
            Phase = DebatePhase.Created,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public DebateState CreateInitialState(Debate debate, List<DebateParticipant> entries,
        Dictionary<Guid, string> introductions)
    {
        return new DebateState
        {
            DebateId = debate.Id,
            Topic = debate.Question,
            Context = debate.Context ?? string.Empty,
            MemberCount = debate.NMembers,
            TotalRounds = debate.NRounds,
            Members = entries
                .OrderBy(e => e.SlotIndex)
                .Select(e => new CouncilMember
                {
                    Slot = e.SlotIndex,
                    Name = e.Name,
                    Role = e.Role,
                    Model = e.Model,
                    Temperature = e.Temperature,
                    Introduction = introductions.GetValueOrDefault(e.MemberId, "Introduction unavailable.")
                }).ToArray()
        };
    }
}
