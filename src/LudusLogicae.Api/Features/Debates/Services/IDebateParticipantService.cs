namespace LudusLogicae.Api.Features.Debates.Services;

public interface IDebateParticipantService
{
    Task AssignMembersAsync(
        Guid debateId,
        List<Guid>? memberIds,
        int nMembers,
        CancellationToken cancellationToken);
    Task AssignMembersWithoutSaveAsync(
        Guid debateId,
        List<Guid>? memberIds,
        int nMembers,
        CancellationToken cancellationToken);
    Task ReplaceMembersAsync(
        Guid debateId,
        List<Guid>? memberIds,
        int nMembers,
        CancellationToken cancellationToken);
    Task RemoveMembersAsync(
        Guid debateId,
        CancellationToken cancellationToken);
}
