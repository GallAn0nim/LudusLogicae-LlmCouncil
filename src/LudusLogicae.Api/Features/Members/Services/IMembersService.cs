using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Members.Services;

public interface IMembersService
{
    Task<ServiceResult<MemberIntroductionResponse>>
        IntroduceAsync(Guid id, CancellationToken cancellationToken);

    Task<ServiceResult<MembersListResponse>>
        ListAsync(MemberListQuery queryParams, CancellationToken cancellationToken);

    Task<ServiceResult<MemberDto>>
        GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ServiceResult<MemberDto>>
        CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<MemberDto>>
        UpdateAsync(Guid id, UpdateMemberRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<MemberDto>>
        DeleteAsync(Guid id, CancellationToken cancellationToken);
}
