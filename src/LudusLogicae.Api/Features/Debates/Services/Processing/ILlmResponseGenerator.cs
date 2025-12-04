using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Processing;

public interface ILlmResponseGenerator
{
    Task<PhaseGenerationResult> GenerateResponsesInParallelAsync<TInput>(
        List<(int SlotIndex, string Model, double Temperature, string SystemPrompt, TInput Input)> requestInputs,
        int maxOutputTokens,
        string phaseDescription,
        CancellationToken cancellationToken);
}
