using LudusLogicae.Api.Features.Debates.Entities;

namespace LudusLogicae.Api.Features.Debates.Contracts;

public static class DebateMappingExtensions
{
    public static DebateResponse ToResponse(this Debate debate)
    {
        var response = new DebateResponse
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

        if (response.State?.Results is { Length: > 0 } results)
        {
            response.State.Results = results.OrderByDescending(r => r.Score).ToArray();

            var scores = response.State.Results.ToDictionary(r => r.Slot, r => r.Score);
            if (response.State.Members.Length > 0)
            {
                response.State.Members = response.State.Members
                    .OrderByDescending(m => scores.GetValueOrDefault(m.Slot, 0))
                    .ToArray();
            }
        }

        return response;
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
