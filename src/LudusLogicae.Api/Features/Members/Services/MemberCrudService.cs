using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Core.Extensions;
using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Features.Members.Services.Data;
using LudusLogicae.Api.Features.Members.Validation;
using LudusLogicae.Api.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Features.Members.Services;

public sealed class MemberCrudService(
    IMemberRepository repository,
    IMemberValidator validator) : IMemberCrudService
{
    public async Task<ServiceResult<MemberDto>> CreateAsync(
        CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var validation = validator.ValidateCreateRequest(request);
        var validationError = validation.ToBadRequestIfInvalid<MemberDto>();
        if (validationError is not null)
        {
            return validationError;
        }

        var memberEntity = MapRequestToEntity(request);

        repository.Add(memberEntity);
        await repository.SaveChangesAsync(cancellationToken);

        return ResponseFactory.Created(memberEntity.ToMemberDto()) with { CreatedId = memberEntity.Id };
    }

    public async Task<ServiceResult<MemberDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var member = await repository.GetByIdAsync(id, cancellationToken);

        return member?.ToOkResponse(m => m.ToMemberDto()) ?? member.ToNotFoundResponse<Member, MemberDto>("Member", id);
    }

    public async Task<ServiceResult<MembersListResponse>> ListAsync(
        MemberListQuery queryParams,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(PaginationConstants.DefaultPage, queryParams.Page);
        var pageSize = queryParams.PageSize is < PaginationConstants.MinPageSize or > PaginationConstants.MaxPageSize
            ? PaginationConstants.DefaultPageSize
            : queryParams.PageSize;

        var query = repository.BuildListQuery(
            queryParams.Status,
            queryParams.Provider,
            queryParams.Model,
            queryParams.SearchTerm,
            queryParams.MinWins,
            queryParams.MinPoints);

        var sortOption = MemberSortHelper.ParseSortOption(queryParams.SortBy);
        query = MemberSortHelper.ApplySorting(query, sortOption);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(member => member.ToMemberDto())
            .ToListAsync(cancellationToken);

        var payload = new MembersListResponse
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        return ResponseFactory.Ok(payload);
    }

    public async Task<ServiceResult<MemberDto>> UpdateAsync(
        Guid id,
        UpdateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var validation = validator.ValidateUpdateRequest(request);
        var validationError = validation.ToBadRequestIfInvalid<MemberDto>();
        if (validationError is not null)
        {
            return validationError;
        }

        var memberEntity = await repository.GetByIdAsync(id, cancellationToken, false);

        if (memberEntity is null)
        {
            return memberEntity.ToNotFoundResponse<Member, MemberDto>("Member", id);
        }

        repository.ApplyUpdates(memberEntity, request);

        await repository.SaveChangesAsync(cancellationToken);
        return ResponseFactory.Ok(memberEntity.ToMemberDto());
    }

    public async Task<ServiceResult<MemberDto>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);

        return deleted == 0 ? ((Member?)null).ToNotFoundResponse<Member, MemberDto>("Member", id) : ResponseFactory.NoContent<MemberDto>();
    }

    private static Member MapRequestToEntity(CreateMemberRequest request)
    {
        return new Member
        {
            Id = Guid.NewGuid(),
            Name = request.Name.TrimOrEmpty(),
            Role = request.Role.TrimOrEmpty(),
            StyleHint = request.StyleHint.TrimOrEmpty(),
            BehaviorExtras = request.BehaviorExtras.TrimOrEmpty(),
            Provider = request.Provider.TrimOrEmpty(),
            Model = request.Model.TrimOrEmpty(),
            Temperature = request.Temperature,
            Status = request.Status,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
