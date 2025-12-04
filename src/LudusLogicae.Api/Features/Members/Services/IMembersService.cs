using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Members.Services;

public interface IMembersService
{
    Task<(int StatusCode, LudusLogicaeResponse<MemberIntroductionResponse> Envelope)>
        IntroduceAsync(Guid id, CancellationToken cancellationToken);

    Task<(int StatusCode, LudusLogicaeResponse<MembersListResponse> Envelope)>
        ListAsync(MemberListQuery queryParams, CancellationToken cancellationToken);

    Task<(int StatusCode, LudusLogicaeResponse<MemberDto> Envelope)>
        GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(int StatusCode, LudusLogicaeResponse<MemberDto> Envelope, Guid? CreatedId)>
        CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken);

    Task<(int StatusCode, LudusLogicaeResponse<MemberDto> Envelope)>
        UpdateAsync(Guid id, UpdateMemberRequest request, CancellationToken cancellationToken);

    Task<(int StatusCode, LudusLogicaeResponse<MemberDto>? Envelope)>
        DeleteAsync(Guid id, CancellationToken cancellationToken);
}
