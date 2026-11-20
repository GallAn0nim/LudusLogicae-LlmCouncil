using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Debates.Services;

public interface IDebateService
{
    Task<ServiceResult<DebateResponse>> CreateAsync(
        CreateDebateRequest request, CancellationToken ct);

    Task<ServiceResult<DebateResponse>> GetByIdAsync(Guid id, CancellationToken ct);

    Task<ServiceResult<List<DebateResponse>>> ListAsync(string? status, int page,
        int pageSize, CancellationToken ct);

    Task<ServiceResult<DebateResponse>> UpdateAsync(Guid id,
        UpdateDebateRequest request, CancellationToken ct);

    Task<ServiceResult<string?>> DeleteAsync(Guid id, CancellationToken ct);

    Task<ServiceResult<DebateState>> FullyRunDebateAsync(Guid id,
        CancellationToken ct);
}
