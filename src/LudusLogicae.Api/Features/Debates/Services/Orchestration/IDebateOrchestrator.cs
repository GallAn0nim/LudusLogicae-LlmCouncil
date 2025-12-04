using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Debates.Services.Orchestration;

public interface IDebateOrchestrator
{
    List<DebateParticipant> AssignMembersToDebate(Guid debateId, List<Member> members);
}
