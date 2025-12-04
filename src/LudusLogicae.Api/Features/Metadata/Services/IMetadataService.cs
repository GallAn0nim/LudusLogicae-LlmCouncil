using LudusLogicae.Api.Features.Metadata.Contracts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Metadata.Services;

public interface IMetadataService
{
    Task<(int StatusCode, LudusLogicaeResponse<string> Envelope)> PingAsync(CancellationToken ct);

    Task<(int StatusCode, LudusLogicaeResponse<MetadataInfoResponse> Envelope)> InfoAsync(CancellationToken ct);

    Task<(int StatusCode, LudusLogicaeResponse<List<string>> Envelope)> GetProvidersAsync(CancellationToken ct);

    Task<(int StatusCode, LudusLogicaeResponse<List<string>> Envelope)> GetProviderModelsAsync(string id,
        CancellationToken ct);

    Task<(int StatusCode, LudusLogicaeResponse<List<string>> Envelope)> GetDebatePhaseesAsync(CancellationToken ct);

    Task<(int StatusCode, LudusLogicaeResponse<Dictionary<string, string>> Envelope)> GetMemberSortOptionsAsync(
        CancellationToken ct);
}
