using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Members.Services;

public interface IMemberSelectionService
{
    Task<List<Member>> ChooseMembersForDebateAsync(List<Guid>? requestedIds, int requiredCount, CancellationToken ct);
    Task<List<Member>> GetMembersByIdsAsync(List<Guid> memberIds, CancellationToken ct);
}
