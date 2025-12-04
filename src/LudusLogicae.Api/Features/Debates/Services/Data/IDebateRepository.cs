using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Data;

public interface IDebateRepository
{
    IQueryable<Debate> BuildListQuery(string? status);

    Task<List<DebateParticipant>> GetParticipantsByDebateIdAsync(
        Guid debateId,
        CancellationToken cancellationToken);

    Task<List<DebateParticipant>> ReplaceParticipantsAsync(
        Guid debateId,
        List<DebateParticipant> newParticipants,
        CancellationToken cancellationToken);

    Task<int> RemoveParticipantsAsync(
        Guid debateId,
        CancellationToken cancellationToken);

    Debate ApplyUpdates(Debate debate, UpdateDebateRequest request);

    Task SaveStateAsync(
        Debate debate,
        DebateState state,
        CancellationToken cancellationToken);

    Task SaveStateAsync(
        Debate debate,
        DebateState state,
        DebatePhase phase,
        CancellationToken cancellationToken);

    void AddParticipants(List<DebateParticipant> participants);

    void Add(Debate entity);
    void Remove(Debate entity);
    Task<Debate?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = true);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
