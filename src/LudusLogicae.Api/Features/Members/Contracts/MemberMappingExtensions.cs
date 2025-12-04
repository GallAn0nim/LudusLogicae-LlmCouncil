using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Members.Contracts;

public static class MemberMappingExtensions
{
    public static MemberDto ToMemberDto(this Member entity)
    {
        return new MemberDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Role = entity.Role,
            Provider = entity.Provider,
            Model = entity.Model,
            Temperature = entity.Temperature,
            StyleHint = entity.StyleHint,
            BehaviorExtras = entity.BehaviorExtras,
            Status = entity.Status,
            Points = entity.Points,
            DebatesCount = entity.DebatesCount,
            WinsCount = entity.WinsCount,
            LastPlaceCount = entity.LastPlaceCount,
            LastDebateDate = entity.LastDebateDate,
            CreatedAt = entity.CreatedAt
        };
    }

    public static Member ToEntity(this CreateMemberRequest request)
    {
        return new Member
        {
            Name = request.Name.Trim(),
            Role = request.Role.Trim(),
            Provider = request.Provider.Trim(),
            Model = request.Model.Trim(),
            Temperature = request.Temperature,
            StyleHint = request.StyleHint?.Trim() ?? string.Empty,
            BehaviorExtras = request.BehaviorExtras?.Trim() ?? string.Empty,
            Status = request.Status,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static void ApplyUpdate(this Member entity, UpdateMemberRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.Role = request.Role.Trim();
        entity.Provider = request.Provider.Trim();
        entity.Model = request.Model.Trim();
        entity.Temperature = request.Temperature;
        entity.StyleHint = request.StyleHint?.Trim() ?? string.Empty;
        entity.BehaviorExtras = request.BehaviorExtras?.Trim() ?? string.Empty;
        entity.Status = request.Status;
    }
}
