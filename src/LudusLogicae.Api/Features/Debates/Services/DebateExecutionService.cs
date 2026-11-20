using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Core.Extensions;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services.Data;
using LudusLogicae.Api.Features.Debates.Validation;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Debates.Services;

public sealed class DebateExecutionService(
    IDebateRepository repository,
    IDebateValidator validator,
    IDebatePhaseCoordinator phaseCoordinator,
    IDebateStateManager stateManager,
    IDebateCompletionService completionService) : IDebateExecutionService
{
    public async Task<ServiceResult<DebateState>> ExecuteDebateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var debate = await repository.GetByIdAsync(id, cancellationToken, false);
        if (debate is null)
        {
            return debate.ToNotFoundResponse<Debate, DebateState>("Debate", id);
        }

        var editableError = DebateValidationHelper.ValidateEditability<DebateState>(
            debate,
            validator,
            DebateOperationType.Start);
        if (editableError is not null)
        {
            return editableError;
        }

        var participants = await repository.GetParticipantsByDebateIdAsync(id, cancellationToken);
        if (participants.Count == 0)
        {
            return ResponseFactory.BadRequest<DebateState>(
                ErrorMessages.MissingRequired("Members", "none are assigned to this debate"));
        }

        var memberIds = participants.Select(e => e.MemberId).ToList();
        var state = await completionService.InitializeDebateAsync(
            debate,
            participants,
            memberIds,
            cancellationToken);

        debate.State = state;
        await stateManager.PersistStateAsync(debate, state, cancellationToken);

        await phaseCoordinator.ExecuteAllPhasesAsync(debate, participants, state, cancellationToken);
        await stateManager.PersistFinalStateAsync(debate, state, cancellationToken);
        await completionService.FinalizeDebateAsync(participants, state, cancellationToken);

        return ResponseFactory.Ok(state);
    }
}
