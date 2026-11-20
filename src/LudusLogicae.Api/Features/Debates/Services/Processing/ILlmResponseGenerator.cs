using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Processing;

public interface ILlmResponseGenerator
{
    Task<PhaseGenerationResult> GenerateResponsesInParallelAsync<TInput>(
        List<PhaseRequest<TInput>> requestInputs,
        int maxOutputTokens,
        string phaseDescription,
        CancellationToken cancellationToken);
}
