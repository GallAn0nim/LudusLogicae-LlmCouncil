using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Debates.Services;

public interface IDebateCrudService
{
    Task<(int StatusCode, LudusLogicaeResponse<DebateResponse> Envelope, Guid? CreatedId)> CreateAsync(
        CreateDebateRequest request,
        CancellationToken ct);
    Task<(int StatusCode, LudusLogicaeResponse<DebateResponse> Envelope, Guid? CreatedId)> CreateWithoutSaveAsync(
        CreateDebateRequest request,
        CancellationToken ct);
    Task<(int StatusCode, LudusLogicaeResponse<DebateResponse> Envelope)> GetByIdAsync(
        Guid id,
        CancellationToken ct);
    Task<(int StatusCode, LudusLogicaeResponse<List<DebateResponse>> Envelope)> ListAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct);
    Task<(int StatusCode, LudusLogicaeResponse<DebateResponse> Envelope)> UpdateAsync(
        Guid id,
        UpdateDebateRequest request,
        CancellationToken ct);
    Task<(int StatusCode, LudusLogicaeResponse<string?> Envelope)> DeleteAsync(
        Guid id,
        CancellationToken ct);
}
