using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Data;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Debates.Services;

public sealed class DebateService(
    IDebateCrudService crudService,
    IDebateParticipantService participantService,
    IDebateExecutionService executionService,
    IDebateRepository repository) : IDebateService
{
    public async Task<ServiceResult<DebateResponse>> CreateAsync(
        CreateDebateRequest request,
        CancellationToken ct)
    {
        var result = await crudService.CreateWithoutSaveAsync(request, ct);

        if (result.StatusCode == 201 && result.CreatedId.HasValue)
        {
            await participantService.AssignMembersWithoutSaveAsync(
                result.CreatedId.Value,
                request.MemberIds,
                request.NMembers,
                ct);

            await repository.SaveChangesAsync(ct);
        }

        return result;
    }

    public Task<ServiceResult<DebateResponse>> GetByIdAsync(
        Guid id,
        CancellationToken ct)
    {
        return crudService.GetByIdAsync(id, ct);
    }

    public Task<ServiceResult<List<DebateResponse>>> ListAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        return crudService.ListAsync(status, page, pageSize, ct);
    }

    public async Task<ServiceResult<DebateResponse>> UpdateAsync(
        Guid id,
        UpdateDebateRequest request,
        CancellationToken ct)
    {
        var debate = await crudService.GetByIdAsync(id, ct);
        if (debate.StatusCode != 200)
        {
            return debate;
        }

        var effectiveMemberCount = request.NMembers ?? debate.Envelope.Data.NMembers;

        if (request.MemberIds != null ||
            (request.NMembers.HasValue && request.NMembers.Value != debate.Envelope.Data.NMembers))
        {
            await participantService.ReplaceMembersAsync(
                id,
                request.MemberIds,
                effectiveMemberCount,
                ct);
        }

        return await crudService.UpdateAsync(id, request, ct);
    }

    public async Task<ServiceResult<string?>> DeleteAsync(
        Guid id,
        CancellationToken ct)
    {
        await participantService.RemoveMembersAsync(id, ct);
        return await crudService.DeleteAsync(id, ct);
    }

    public Task<ServiceResult<DebateState>> FullyRunDebateAsync(
        Guid id,
        CancellationToken ct)
    {
        return executionService.ExecuteDebateAsync(id, ct);
    }
}
