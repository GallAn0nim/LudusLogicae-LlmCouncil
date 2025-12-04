using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Members.Services;

public sealed class MembersService(
    IMemberCrudService crudService,
    IMemberIntroductionService introductionService) : IMembersService
{
    public Task<(int StatusCode, LudusLogicaeResponse<MemberIntroductionResponse> Envelope)> IntroduceAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return introductionService.GenerateIntroductionAsync(id, cancellationToken);
    }

    public Task<(int StatusCode, LudusLogicaeResponse<MembersListResponse> Envelope)> ListAsync(
        MemberListQuery queryParams,
        CancellationToken cancellationToken)
    {
        return crudService.ListAsync(queryParams, cancellationToken);
    }

    public Task<(int StatusCode, LudusLogicaeResponse<MemberDto> Envelope)> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return crudService.GetByIdAsync(id, cancellationToken);
    }

    public Task<(int StatusCode, LudusLogicaeResponse<MemberDto> Envelope, Guid? CreatedId)> CreateAsync(
        CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        return crudService.CreateAsync(request, cancellationToken);
    }

    public Task<(int StatusCode, LudusLogicaeResponse<MemberDto> Envelope)> UpdateAsync(
        Guid id,
        UpdateMemberRequest request,
        CancellationToken cancellationToken)
    {
        return crudService.UpdateAsync(id, request, cancellationToken);
    }

    public Task<(int StatusCode, LudusLogicaeResponse<MemberDto>? Envelope)> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return crudService.DeleteAsync(id, cancellationToken);
    }
}
