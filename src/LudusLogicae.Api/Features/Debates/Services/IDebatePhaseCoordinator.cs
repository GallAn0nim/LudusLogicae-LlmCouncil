using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services;

public interface IDebatePhaseCoordinator
{
    Task ExecuteAllPhasesAsync(
        Debate debate,
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken);
}
