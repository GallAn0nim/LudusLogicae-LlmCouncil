using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Debates.Entities;

public class DebateParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DebateId { get; set; }
    public Guid MemberId { get; set; }
    public int SlotIndex { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string StyleHint { get; set; } = string.Empty;
    public string BehaviorExtras { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; }

    public string? InitialAnswer { get; set; }
    public List<CrossQuestion> CrossQuestions { get; set; } = [];
    public List<QuestionResponse> QuestionResponses { get; set; } = [];
    public string? Amendment { get; set; }
    public string? FinalAnswer { get; set; }
    public List<int>? BallotRanking { get; set; }

    public double BordaScore { get; set; }
    public double FinalScore { get; set; }
    public bool IsWinner { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Debate Debate { get; set; } = null!;
    public Member Member { get; set; } = null!;
}

public record CrossQuestion(int ToSlot, string Question);

public record QuestionResponse(int FromSlot, string Answer);
