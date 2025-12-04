using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Infrastructure.Llm.Services;

namespace LudusLogicae.Api.Features.Debates.Factories;

public sealed class DebateParticipantFactory(ILlmProviderService llmProviderService)
{
    public DebateParticipant CreateParticipant(Guid debateId, Member member, int slotIndex)
    {
        var validatedProvider = llmProviderService.ValidateAndFallbackProvider(member.Provider);
        var validatedModel = llmProviderService.ValidateAndFallbackModel(validatedProvider, member.Model);

        return new DebateParticipant
        {
            DebateId = debateId,
            SlotIndex = slotIndex,
            MemberId = member.Id,
            Name = member.Name,
            Role = member.Role,
            StyleHint = member.StyleHint,
            BehaviorExtras = member.BehaviorExtras ?? string.Empty,
            Model = validatedModel,
            Temperature = member.Temperature,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
