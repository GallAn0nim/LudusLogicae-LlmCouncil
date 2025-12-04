using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Members.Services.Data;

public interface IMemberRepository
{
    IQueryable<Member> BuildListQuery(
        string? status,
        string? provider,
        string? model,
        string? searchTerm,
        int? minWins,
        int? minPoints);

    Member ApplyUpdates(Member member, UpdateMemberRequest request);

    Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken);
    void Add(Member entity);
    void Remove(Member entity);
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = true);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
