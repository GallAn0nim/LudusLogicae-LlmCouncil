using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Features.Members.Services;

public sealed class MemberSelectionService(AppDbContext db) : IMemberSelectionService
{
    public async Task<List<Member>> ChooseMembersForDebateAsync(List<Guid>? requestedIds, int requiredCount,
        CancellationToken ct)
    {
        var chosenMembers = new List<Member>();

        if (requestedIds != null && requestedIds.Count > 0)
        {
            var distinctIds = requestedIds.Distinct().Take(requiredCount).ToList();
            var foundMembers = await db.Members.AsNoTracking()
                .Where(a => distinctIds.Contains(a.Id))
                .ToListAsync(ct);

            var benchedCount = 0;
            foreach (var member in foundMembers)
                if (member.Status == MemberStatus.Benched)
                {
                    if (benchedCount == 0)
                    {
                        chosenMembers.Add(member);
                        benchedCount++;
                    }
                }
                else
                {
                    chosenMembers.Add(member);
                }
        }

        var missingCount = requiredCount - chosenMembers.Count;
        if (missingCount > 0)
        {
            var existingIds = chosenMembers.Select(a => a.Id).ToList();
            var hasBenched = chosenMembers.Any(a => a.Status == MemberStatus.Benched);

            var randomMembers = await db.Members
                .AsNoTracking()
                .Where(m => !existingIds.Contains(m.Id))
                .Where(m => m.Status == MemberStatus.Active || m.Status == MemberStatus.Benched)
                .Select(m => new
                {
                    Member = m,
                    RandomValue = EF.Functions.Random() * (m.Status == MemberStatus.Active ? 2.0 : 1.0)
                })
                .OrderByDescending(x => x.RandomValue)
                .Take(missingCount + 1)
                .Select(x => x.Member)
                .ToListAsync(ct);
            var candidates = new List<Member>();
            if (!hasBenched)
            {
                var benchedMember = randomMembers.FirstOrDefault(m => m.Status == MemberStatus.Benched);
                if (benchedMember != null)
                {
                    candidates.Add(benchedMember);
                    randomMembers = randomMembers.Where(m => m.Id != benchedMember.Id).ToList();
                }
            }

            candidates.AddRange(randomMembers.Take(missingCount - candidates.Count));
            chosenMembers.AddRange(candidates);
        }

        return chosenMembers.Take(requiredCount).ToList();
    }

    public async Task<List<Member>> GetMembersByIdsAsync(List<Guid> memberIds, CancellationToken ct)
    {
        if (memberIds == null || memberIds.Count == 0) return [];

        var distinctIds = memberIds.Distinct().ToList();
        return await db.Members.AsNoTracking()
            .Where(a => distinctIds.Contains(a.Id))
            .ToListAsync(ct);
    }
}
