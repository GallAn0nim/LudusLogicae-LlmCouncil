using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services;

public interface IDebateStateManager
{
    Task PersistStateAsync(
         Debate debate,
         DebateState state,
         CancellationToken cancellationToken);
    Task PersistFinalStateAsync(
        Debate debate,
        DebateState state,
        CancellationToken cancellationToken);
}
