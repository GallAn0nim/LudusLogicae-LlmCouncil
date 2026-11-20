using LudusLogicae.Api.Core.Data;
using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Features.Debates.Services.Data;

public sealed class DebateRepository(AppDbContext dbContext) : BaseRepository<Debate>(dbContext), IDebateRepository
{
    public IQueryable<Debate> BuildListQuery(string? status)
    {
        var query = dbContext.Debates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DebatePhase>(status, true, out var statusEnum))
        {
            query = query.Where(debate => debate.Phase == statusEnum);
        }

        return query;
    }

    public async Task<List<DebateParticipant>> GetParticipantsByDebateIdAsync(
        Guid debateId,
        CancellationToken cancellationToken)
    {
        return await dbContext.DebateParticipants
            .AsNoTracking()
            .Where(entry => entry.DebateId == debateId)
            .OrderBy(entry => entry.SlotIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DebateParticipant>> ReplaceParticipantsAsync(
        Guid debateId,
        List<DebateParticipant> newParticipants,
        CancellationToken cancellationToken)
    {
        var existingParticipants = await dbContext.DebateParticipants
            .Where(entry => entry.DebateId == debateId)
            .ToListAsync(cancellationToken);

        var newParticipantsBySlot = newParticipants.ToDictionary(p => p.SlotIndex);
        var existingParticipantsBySlot = existingParticipants.ToDictionary(p => p.SlotIndex);

        var toRemove = existingParticipants
            .Where(existing => !newParticipantsBySlot.ContainsKey(existing.SlotIndex))
            .ToList();

        var toAdd = newParticipants
            .Where(newP => !existingParticipantsBySlot.ContainsKey(newP.SlotIndex))
            .ToList();

        var toUpdate = newParticipants
            .Where(newP => existingParticipantsBySlot.ContainsKey(newP.SlotIndex))
            .ToList();

        if (toRemove.Count > 0)
        {
            dbContext.DebateParticipants.RemoveRange(toRemove);
        }

        if (toAdd.Count > 0)
        {
            dbContext.DebateParticipants.AddRange(toAdd);
        }

        foreach (var updated in toUpdate)
        {
            var existing = existingParticipantsBySlot[updated.SlotIndex];
            existing.MemberId = updated.MemberId;
            existing.Name = updated.Name;
            existing.Role = updated.Role;
            existing.StyleHint = updated.StyleHint;
            existing.BehaviorExtras = updated.BehaviorExtras;
            existing.Provider = updated.Provider;
            existing.Model = updated.Model;
            existing.Temperature = updated.Temperature;
        }

        return newParticipants;
    }

    public async Task<int> RemoveParticipantsAsync(
        Guid debateId,
        CancellationToken cancellationToken)
    {
        return await dbContext.DebateParticipants
            .Where(entry => entry.DebateId == debateId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public Debate ApplyUpdates(Debate debate, UpdateDebateRequest request)
    {
        if (request.Question != null)
        {
            debate.Question = request.Question;
        }

        if (request.NMembers.HasValue)
        {
            debate.NMembers = request.NMembers.Value;
        }

        if (request.NRounds.HasValue)
        {
            debate.NRounds = request.NRounds.Value;
        }

        if (request.Context != null)
        {
            debate.Context = request.Context;
        }

        return debate;
    }

    public async Task SaveStateAsync(
        Debate debate,
        DebateState state,
        CancellationToken cancellationToken)
    {
        debate.State = state;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveStateAsync(
        Debate debate,
        DebateState state,
        DebatePhase phase,
        CancellationToken cancellationToken)
    {
        debate.Phase = phase;
        debate.State = state;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void AddParticipants(List<DebateParticipant> participants)
    {
        dbContext.DebateParticipants.AddRange(participants);
    }
}
