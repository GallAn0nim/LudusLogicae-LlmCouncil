using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Members.Services;

public interface IMemberStatsUpdater
{
    Task UpdateStatsAfterDebateAsync(
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken);
}
