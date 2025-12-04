namespace LudusLogicae.Api.Features.Debates.Contracts;

public class DebateParticipantDto
{
    public Guid Id { get; set; }
    public Guid DebateId { get; set; }
    public int SlotIndex { get; set; }
    public Guid? MemberId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string InitialAnswer { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
}
