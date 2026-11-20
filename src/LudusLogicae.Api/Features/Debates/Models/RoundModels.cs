namespace LudusLogicae.Api.Features.Debates.Models;

public sealed record RawPhaseResult(int Slot, string RawText);

public sealed record PhaseRequest<TInput>(
    int SlotIndex,
    string Provider,
    string Model,
    double Temperature,
    string SystemPrompt,
    TInput Input);

public sealed class DebateRound
{
    public int RoundNumber { get; init; }

    public RoundPairing[] Pairings { get; set; } = [];

    public RoundQuestion[] Questions { get; set; } = [];

    public RoundAnswer[] Answers { get; set; } = [];

    public RoundAmendment[] Amendments { get; set; } = [];
}

public sealed class RoundPairing
{
    public int FromSlot { get; init; }

    public int ToSlot { get; init; }
}

public sealed class RoundQuestion
{
    public int FromSlot { get; init; }

    public int ToSlot { get; init; }

    public string Text { get; init; } = string.Empty;
}

public sealed class RoundAnswer
{
    public int Slot { get; init; }

    public string Text { get; init; } = string.Empty;
}

public sealed class RoundAmendment
{
    public int Slot { get; init; }

    public string Text { get; init; } = string.Empty;
}
