using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Members.Services;

public sealed class MembersService(
    IMemberCrudService crudService,
    IMemberIntroductionService introductionService) : IMembersService
{
    public Task<ServiceResult<MemberIntroductionResponse>> IntroduceAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return introductionService.GenerateIntroductionAsync(id, cancellationToken);
    }

    public Task<ServiceResult<MembersListResponse>> ListAsync(
        MemberListQuery queryParams,
        CancellationToken cancellationToken)
    {
        return crudService.ListAsync(queryParams, cancellationToken);
    }

    public Task<ServiceResult<MemberDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return crudService.GetByIdAsync(id, cancellationToken);
    }

    public Task<ServiceResult<MemberDto>> CreateAsync(
        CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        return crudService.CreateAsync(request, cancellationToken);
    }

    public Task<ServiceResult<MemberDto>> UpdateAsync(
        Guid id,
        UpdateMemberRequest request,
        CancellationToken cancellationToken)
    {
        return crudService.UpdateAsync(id, request, cancellationToken);
    }

    public Task<ServiceResult<MemberDto>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return crudService.DeleteAsync(id, cancellationToken);
    }
}
