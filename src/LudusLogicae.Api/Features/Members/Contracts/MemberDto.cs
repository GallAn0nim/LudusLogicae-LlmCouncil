using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Members.Contracts;

public class MemberDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string StyleHint { get; set; } = string.Empty;
    public string? BehaviorExtras { get; set; } = string.Empty;
    public string Provider { get; set; } = "OpenAI";
    public string Model { get; set; } = "gpt-4.1-nano";
    public double Temperature { get; set; } = 0.5;
    public MemberStatus Status { get; set; } = 0;
    public int Points { get; set; }
    public int DebatesCount { get; set; }
    public int WinsCount { get; set; }
    public int LastPlaceCount { get; set; }
    public DateTimeOffset? LastDebateDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
