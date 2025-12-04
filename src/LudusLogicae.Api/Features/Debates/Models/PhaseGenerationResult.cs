namespace LudusLogicae.Api.Features.Debates.Models;

public sealed class PhaseGenerationResult
{
    public List<RawPhaseResult> SuccessfulResponses { get; init; } = [];

    public List<PhaseGenerationFailure> Failures { get; init; } = [];

    public bool IsFullySuccessful => Failures.Count == 0;

    public bool HasAnySuccess => SuccessfulResponses.Count > 0;

    public bool IsCompleteFailure => SuccessfulResponses.Count == 0 && Failures.Count > 0;

    public int TotalCount => SuccessfulResponses.Count + Failures.Count;
}

public sealed record PhaseGenerationFailure(
    int Slot,
    string ErrorMessage,
    Exception Exception
);
