using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Factories;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Features.Members.Services;

namespace LudusLogicae.Api.Features.Debates.Services;

public sealed class DebateCompletionService(
    IMemberSelectionService memberSelectionService,
    IMemberIntroductionFetcher introductionFetcher,
    IMemberStatsUpdater statsUpdater,
    IDebateFactory factory) : IDebateCompletionService
{
    public async Task FinalizeDebateAsync(
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken)
    {
        await statsUpdater.UpdateStatsAfterDebateAsync(participants, state, cancellationToken);
    }

    public async Task<(DebateState State, List<Member> Members)> InitializeDebateAsync(
        Debate debate,
        List<DebateParticipant> participants,
        List<Guid> memberIds,
        CancellationToken cancellationToken)
    {
        var fetchedMembers = await memberSelectionService.GetMembersByIdsAsync(memberIds, cancellationToken);

        var chosenMembers = participants
            .Select(e => fetchedMembers.FirstOrDefault(a => a.Id == e.MemberId))
            .Where(a => a != null)
            .Cast<Member>()
            .ToList();

        debate.Phase = DebatePhase.InitialAnswer;
        var introductions = await introductionFetcher.FetchIntroductionsAsync(chosenMembers, cancellationToken);
        var state = factory.CreateInitialState(debate, participants, introductions);

        return (state, chosenMembers);
    }
}
