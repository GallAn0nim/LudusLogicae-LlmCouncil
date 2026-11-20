using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Features.Members.Services;

public sealed class MemberStatsUpdater(AppDbContext dbContext) : IMemberStatsUpdater
{
    public async Task UpdateStatsAfterDebateAsync(
        List<DebateParticipant> participants,
        DebateState state,
        CancellationToken cancellationToken)
    {
        var members = await FetchMembersForUpdate(participants, cancellationToken);

        foreach (var participant in participants)
        {
            var member = members.FirstOrDefault(m => m.Id == participant.MemberId);
            if (member == null)
            {
                continue;
            }

            UpdateMemberDebateStats(member, participant, state);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<Member>> FetchMembersForUpdate(
        List<DebateParticipant> participants,
        CancellationToken cancellationToken)
    {
        var memberIds = participants.Select(p => p.MemberId).ToList();

        return await dbContext.Members
            .Where(m => memberIds.Contains(m.Id))
            .ToListAsync(cancellationToken);
    }

    private static void UpdateMemberDebateStats(Member member, DebateParticipant participant, DebateState state)
    {
        UpdateBasicDebateStats(member);

        var result = FindMemberResult(state, participant.SlotIndex);
        if (result == null)
        {
            return;
        }

        var pointChange = CalculatePointChange(result.Rank, state.MemberCount, member);
        ApplyPointChange(member, pointChange);
        UpdateMemberStatus(member);
    }

    private static void UpdateBasicDebateStats(Member member)
    {
        member.DebatesCount++;
        member.LastDebateDate = DateTimeOffset.UtcNow;
    }

    private static MemberScore? FindMemberResult(DebateState state, int slotIndex)
    {
        return state.Results.FirstOrDefault(r => r.Slot == slotIndex);
    }

    private static int CalculatePointChange(int rank, int totalMembers, Member member)
    {
        if (rank == 1)
        {
            member.WinsCount++;
            return 3;
        }

        if (rank == totalMembers)
        {
            member.LastPlaceCount++;
            return -2;
        }

        return 1;
    }

    private static void ApplyPointChange(Member member, int pointChange)
    {
        member.Points += pointChange;
    }

    private static void UpdateMemberStatus(Member member)
    {
        member.Status = member.Points switch
        {
            < 1 => MemberStatus.Retired,
            < 6 => MemberStatus.Benched,
            _ => MemberStatus.Active
        };
    }
}
