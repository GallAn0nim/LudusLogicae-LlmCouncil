using LudusLogicae.Api.Core.Data;
using LudusLogicae.Api.Core.Extensions;
using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Features.Members.Services.Data;

public sealed class MemberRepository(AppDbContext dbContext) : BaseRepository<Member>(dbContext), IMemberRepository
{
    public IQueryable<Member> BuildListQuery(
        string? status,
        string? provider,
        string? model,
        string? searchTerm,
        int? minWins,
        int? minPoints)
    {
        var query = dbContext.Members.AsNoTracking().AsQueryable();

        if (TryParseStatus(status, out var statusValue)) query = query.Where(member => member.Status == statusValue);

        if (!string.IsNullOrWhiteSpace(provider)) query = query.Where(member => member.Provider == provider.Trim());

        if (!string.IsNullOrWhiteSpace(model)) query = query.Where(member => member.Model == model.Trim());

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm.Trim()}%";
            query = query.Where(member =>
                EF.Functions.ILike(member.Name, pattern) ||
                EF.Functions.ILike(member.Role, pattern));
        }

        if (minWins.HasValue) query = query.Where(member => member.WinsCount >= minWins.Value);

        if (minPoints.HasValue) query = query.Where(member => member.Points >= minPoints.Value);

        return query;
    }

    public Member ApplyUpdates(Member member, UpdateMemberRequest request)
    {
        member.Name = request.Name.TrimOrEmpty();
        member.Role = request.Role.TrimOrEmpty();
        member.StyleHint = request.StyleHint.TrimOrEmpty();
        member.BehaviorExtras = request.BehaviorExtras.TrimOrEmpty();
        member.Provider = request.Provider.TrimOrEmpty();
        member.Model = request.Model.TrimOrEmpty();
        member.Temperature = request.Temperature;
        member.Status = request.Status;

        return member;
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Members
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static bool TryParseStatus(string? statusText, out MemberStatus parsed)
    {
        if (!string.IsNullOrWhiteSpace(statusText) &&
            Enum.TryParse<MemberStatus>(statusText.Trim(), true, out var value))
        {
            parsed = value;
            return true;
        }

        parsed = default;
        return false;
    }
}
