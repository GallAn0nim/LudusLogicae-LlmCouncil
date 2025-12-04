using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Factories;
using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration;

public sealed class DebateOrchestrator(DebateParticipantFactory participantFactory) : IDebateOrchestrator
{
    public List<DebateParticipant> AssignMembersToDebate(Guid debateId, List<Member> members)
    {
        return members
            .Select((member, index) => participantFactory.CreateParticipant(debateId, member, index + 1))
            .ToList();
    }
}
