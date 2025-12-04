using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Features.Members.Services.Data;

public sealed class MemberIntroductionCacheRepository(AppDbContext dbContext)
{
    public async Task<MemberIntroductionCache?> GetCachedIntroductionAsync(
        Member member,
        CancellationToken cancellationToken)
    {
        var cacheExpiration = DateTimeOffset.UtcNow.AddDays(-MemberConstants.IntroductionCacheDays);

        return await dbContext.MemberIntroductionCaches.AsNoTracking()
            .Where(cache => cache.MemberId == member.Id
                            && cache.Provider == member.Provider
                            && cache.Model == member.Model
                            && cache.CreatedAt >= cacheExpiration)
            .OrderByDescending(cache => cache.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<List<MemberIntroductionCache>> GetCachedIntroductionsByMembersAsync(
        List<Member> members,
        CancellationToken cancellationToken)
    {
        if (!members.Any()) return [];

        var cacheExpiration = DateTimeOffset.UtcNow.AddDays(-MemberConstants.IntroductionCacheDays);
        var memberIds = members.Select(m => m.Id).ToList();

        var allCaches = await dbContext.MemberIntroductionCaches.AsNoTracking()
            .Where(cache => memberIds.Contains(cache.MemberId)
                            && cache.CreatedAt >= cacheExpiration)
            .ToListAsync(cancellationToken);

        var result = new List<MemberIntroductionCache>();
        foreach (var member in members)
        {
            var matchingCache = allCaches
                .Where(cache => cache.MemberId == member.Id
                                && cache.Provider == member.Provider
                                && cache.Model == member.Model)
                .OrderByDescending(cache => cache.CreatedAt)
                .FirstOrDefault();

            if (matchingCache is not null) result.Add(matchingCache);
        }

        return result;
    }

    public async Task<MemberIntroductionCache> SaveOrUpdateAsync(
        Member member,
        string introduction,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.MemberIntroductionCaches
            .Where(cache => cache.MemberId == member.Id
                            && cache.Provider == member.Provider
                            && cache.Model == member.Model)
            .OrderByDescending(cache => cache.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            existing.Introduction = introduction;
            existing.CreatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var cacheEntry = new MemberIntroductionCache
        {
            MemberId = member.Id,
            Provider = member.Provider,
            Model = member.Model,
            Introduction = introduction,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.MemberIntroductionCaches.Add(cacheEntry);
        await dbContext.SaveChangesAsync(cancellationToken);
        return cacheEntry;
    }
}
