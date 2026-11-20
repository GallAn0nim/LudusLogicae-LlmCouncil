using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Members.Services;

public interface IMemberIntroductionService
{
    Task<ServiceResult<MemberIntroductionResponse>> GenerateIntroductionAsync(
        Guid id,
        CancellationToken cancellationToken);
}
