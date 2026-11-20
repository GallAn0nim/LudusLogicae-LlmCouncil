using LudusLogicae.Api.Features.Metadata.Contracts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Metadata.Services;

public interface IMetadataService
{
    Task<ServiceResult<string>> PingAsync(CancellationToken ct);

    Task<ServiceResult<MetadataInfoResponse>> InfoAsync(CancellationToken ct);

    Task<ServiceResult<List<string>>> GetProvidersAsync(CancellationToken ct);

    Task<ServiceResult<List<string>>> GetProviderModelsAsync(string id,
        CancellationToken ct);

    Task<ServiceResult<List<string>>> GetDebatePhaseesAsync(CancellationToken ct);

    Task<ServiceResult<Dictionary<string, string>>> GetMemberSortOptionsAsync(
        CancellationToken ct);
}
