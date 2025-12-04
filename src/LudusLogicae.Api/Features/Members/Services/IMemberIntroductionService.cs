using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Members.Services;

public interface IMemberIntroductionService
{
    Task<(int StatusCode, LudusLogicaeResponse<MemberIntroductionResponse> Envelope)> GenerateIntroductionAsync(
        Guid id,
        CancellationToken cancellationToken);
}
