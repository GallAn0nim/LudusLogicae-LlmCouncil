using LudusLogicae.Api.Features.Debates.Entities;

namespace LudusLogicae.Api.Features.Debates.Contracts;

public static class DebateMappingExtensions
{
    public static DebateResponse ToResponse(this Debate debate)
    {
        return new DebateResponse
        {
            Id = debate.Id,
            Status = debate.Phase,
            Question = debate.Question,
            Context = debate.Context ?? string.Empty,
            NMembers = debate.NMembers,
            NRounds = debate.NRounds,
            CreatedAt = debate.CreatedAt,
            CompletedAt = debate.CompletedAt,
            State = debate.State
        };
    }

    public static List<DebateParticipantDto> ToDtos(this IEnumerable<DebateParticipant> entries)
    {
        return entries.Select(e => new DebateParticipantDto
        {
            Id = e.Id,
            DebateId = e.DebateId,
            SlotIndex = e.SlotIndex,
            MemberId = e.MemberId,
            Name = e.Name,
            Role = e.Role,
            Model = e.Model,
            Temperature = e.Temperature,
            InitialAnswer = e.InitialAnswer ?? string.Empty
        }).ToList();
    }
}
