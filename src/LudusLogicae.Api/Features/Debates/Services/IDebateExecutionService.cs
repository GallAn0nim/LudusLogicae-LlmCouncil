using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Debates.Services;

public interface IDebateExecutionService
{
    Task<(int StatusCode, LudusLogicaeResponse<DebateState> Envelope)> ExecuteDebateAsync(
        Guid id,
        CancellationToken ct);
}
