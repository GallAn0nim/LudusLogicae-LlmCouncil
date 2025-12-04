using System.Globalization;
using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Infrastructure.Llm.Models;

namespace LudusLogicae.Api.Infrastructure.Llm.Prompts;

public class MemberPromptFactory
{
    public LLMRequest BuildIntroductionPrompt(Member member)
    {
        const string systemPrompt = "You are an AI participant in a structured debate council. " +
                                    "Your goal is to introduce yourself based on your assigned persona. " +
                                    "Be concise and stay in character. Embody the role completely; think, speak, and argue like them.";

        var style = string.IsNullOrWhiteSpace(member.StyleHint) ? null : $"Style hint: {member.StyleHint}.";
        var extras = string.IsNullOrWhiteSpace(member.BehaviorExtras)
            ? null
            : $"Behavior extras: {member.BehaviorExtras}.";
        var temperature = string.IsNullOrWhiteSpace(member.Temperature.ToString(CultureInfo.InvariantCulture))
            ? null
            : $"Temperature: {member.Temperature}.";

        var userPrompt =
            "Please introduce yourself in the first person as this council member. Keep it concise (3–6 sentences).\n" +
            $"Name: {member.Name}\n" +
            $"Discipline: {member.Role}\n" +
            (style is not null ? style + "\n" : string.Empty) +
            (extras is not null ? extras + "\n" : string.Empty) +
            (temperature is not null ? temperature + "\n" : string.Empty) +
            "Avoid making up credentials. If uncertain, be honest.";

        return new LLMRequest
        {
            Provider = member.Provider,
            Model = member.Model,
            Temperature = member.Temperature,
            MaxOutputTokens = 10000,
            SystemPrompt = systemPrompt,
            Messages = [new LLMMessage { Role = LLMRole.User, Content = userPrompt }],
            Metadata = new Dictionary<string, string>
            {
                ["Member_id"] = member.Id.ToString(),
                ["purpose"] = "introduction"
            }
        };
    }
}
