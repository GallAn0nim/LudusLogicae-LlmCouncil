using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Debates.Services;

public interface IDebateCompletionService
{

    Task FinalizeDebateAsync(
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken);

    Task<(DebateState State, List<Member> Members)> InitializeDebateAsync(
        Debate debate,
        List<DebateParticipant> participants,
        List<Guid> memberIds,
        CancellationToken cancellationToken);
}
