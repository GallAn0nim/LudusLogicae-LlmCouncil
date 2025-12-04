using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Members.Services;

public interface IMemberIntroductionFetcher
{
    Task<Dictionary<Guid, string>> FetchIntroductionsAsync(
    List<Member> members,
    CancellationToken cancellationToken);
}
