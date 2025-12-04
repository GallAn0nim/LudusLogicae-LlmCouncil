using LudusLogicae.Api.Features.Debates.Services.Data;
using LudusLogicae.Api.Features.Debates.Services.Orchestration;
using LudusLogicae.Api.Features.Members.Services;

namespace LudusLogicae.Api.Features.Debates.Services;

public sealed class DebateParticipantService(
    IDebateRepository repository,
    IMemberSelectionService memberSelectionService,
    IDebateOrchestrator orchestrator) : IDebateParticipantService
{
    public async Task AssignMembersAsync(
        Guid debateId,
        List<Guid>? memberIds,
        int nMembers,
        CancellationToken cancellationToken)
    {
        await AssignMembersWithoutSaveAsync(debateId, memberIds, nMembers, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignMembersWithoutSaveAsync(
        Guid debateId,
        List<Guid>? memberIds,
        int nMembers,
        CancellationToken cancellationToken)
    {
        var chosenMembers = await memberSelectionService.ChooseMembersForDebateAsync(
            memberIds,
            nMembers,
            cancellationToken);

        var participants = orchestrator.AssignMembersToDebate(debateId, chosenMembers);
        repository.AddParticipants(participants);
    }

    public async Task ReplaceMembersAsync(
        Guid debateId,
        List<Guid>? memberIds,
        int nMembers,
        CancellationToken cancellationToken)
    {
        var requestedIds = memberIds ?? [];

        if (requestedIds.Count == 0)
        {
            var existingEntries = await repository.GetParticipantsByDebateIdAsync(debateId, cancellationToken);
            requestedIds = existingEntries.Select(e => e.MemberId).ToList();
        }

        var chosenMembers = await memberSelectionService.ChooseMembersForDebateAsync(
            requestedIds,
            nMembers,
            cancellationToken);

        var newParticipants = orchestrator.AssignMembersToDebate(debateId, chosenMembers);
        await repository.ReplaceParticipantsAsync(debateId, newParticipants, cancellationToken);
    }

    public async Task RemoveMembersAsync(
        Guid debateId,
        CancellationToken cancellationToken)
    {
        await repository.RemoveParticipantsAsync(debateId, cancellationToken);
    }
}
