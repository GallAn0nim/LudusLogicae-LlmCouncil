using LudusLogicae.Api.Core.Extensions;
using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Factories;
using LudusLogicae.Api.Features.Debates.Services.Data;
using LudusLogicae.Api.Features.Debates.Validation;
using LudusLogicae.Api.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Features.Debates.Services;

public sealed class DebateCrudService(
    IDebateRepository repository,
    IDebateValidator validator,
    IDebateFactory factory) : IDebateCrudService
{
    public async Task<ServiceResult<DebateResponse>> CreateAsync(
        CreateDebateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await CreateWithoutSaveAsync(request, cancellationToken);

        if (result.StatusCode == 201)
        {
            await repository.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public Task<ServiceResult<DebateResponse>>
        CreateWithoutSaveAsync(
            CreateDebateRequest request,
            CancellationToken cancellationToken)
    {
        var validation = validator.ValidateDebateConfiguration(request.NMembers);
        var validationError = validation.ToBadRequestIfInvalid<DebateResponse>();
        if (validationError is not null)
        {
            return Task.FromResult(validationError);
        }

        var debate = factory.CreateDebateEntity(request);
        repository.Add(debate);

        return Task.FromResult(ResponseFactory.Ok(debate.ToResponse()) with { StatusCode = 201, CreatedId = debate.Id });
    }

    public async Task<ServiceResult<DebateResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var debate = await repository.GetByIdAsync(id, cancellationToken);
        return debate?.ToOkResponse(d => d.ToResponse()) ?? debate.ToNotFoundResponse<Debate, DebateResponse>("Debate", id);
    }

    public async Task<ServiceResult<List<DebateResponse>>> ListAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = repository.BuildListQuery(status);

        var items = await query
            .OrderByDescending(debate => debate.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ResponseFactory.Ok(items.Select(debate => debate.ToResponse()).ToList());
    }

    public async Task<ServiceResult<DebateResponse>> UpdateAsync(
        Guid id,
        UpdateDebateRequest request,
        CancellationToken cancellationToken)
    {
        var debate = await repository.GetByIdAsync(id, cancellationToken, false);
        if (debate is null)
        {
            return debate.ToNotFoundResponse<Debate, DebateResponse>("Debate", id);
        }

        var editableError = DebateValidationHelper.ValidateEditability<DebateResponse>(
            debate,
            validator,
            DebateOperationType.Update);
        if (editableError is not null)
        {
            return editableError;
        }

        var effectiveMemberCount = request.NMembers ?? debate.NMembers;

        var validation = validator.ValidateDebateConfiguration(effectiveMemberCount);
        var validationError = validation.ToBadRequestIfInvalid<DebateResponse>();
        if (validationError is not null)
        {
            return validationError;
        }

        var updatedDebate = repository.ApplyUpdates(debate, request);
        await repository.SaveChangesAsync(cancellationToken);

        return ResponseFactory.Ok(updatedDebate.ToResponse());
    }

    public async Task<ServiceResult<string?>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var debate = await repository.GetByIdAsync(id, cancellationToken, false);
        if (debate is null)
        {
            return debate.ToNotFoundResponse<Debate, string?>("Debate", id);
        }

        repository.Remove(debate);
        await repository.SaveChangesAsync(cancellationToken);

        return ResponseFactory.NoContent<string?>();
    }
}
