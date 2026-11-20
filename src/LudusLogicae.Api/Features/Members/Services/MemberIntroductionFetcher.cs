using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Features.Members.Services.Data;

namespace LudusLogicae.Api.Features.Members.Services;

public sealed class MemberIntroductionFetcher(
    IMembersService membersService,
    MemberIntroductionCacheRepository cacheRepository) : IMemberIntroductionFetcher
{
    public async Task<Dictionary<Guid, string>> FetchIntroductionsAsync(
        List<Member> members,
        CancellationToken cancellationToken)
    {
        if (!members.Any())
        {
            return new Dictionary<Guid, string>();
        }

        var cachedIntros = await cacheRepository.GetCachedIntroductionsByMembersAsync(members, cancellationToken);
        var cachedDict = cachedIntros.ToDictionary(c => c.MemberId, c => c.Introduction);

        var missingMembers = members.Where(m => !cachedDict.ContainsKey(m.Id)).ToList();

        if (missingMembers.Any())
        {
            foreach (var member in missingMembers)
            {
                var intro = await FetchSingleIntroductionAsync(member.Id, cancellationToken);
                cachedDict[member.Id] = intro;
            }
        }

        return cachedDict;
    }

    private async Task<string> FetchSingleIntroductionAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var introResponse = await membersService.IntroduceAsync(memberId, cancellationToken);

        return introResponse.StatusCode == 200
            ? introResponse.Envelope.Data.Introduction
            : "Introduction unavailable.";
    }
}
