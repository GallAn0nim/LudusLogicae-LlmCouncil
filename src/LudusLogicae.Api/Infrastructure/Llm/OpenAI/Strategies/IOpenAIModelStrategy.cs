using LudusLogicae.Api.Infrastructure.Llm.Models;
using OpenAI;

namespace LudusLogicae.Api.Infrastructure.Llm.OpenAI.Strategies;

public interface IOpenAIModelStrategy
{
    Task<LLMResponse> ExecuteAsync(OpenAIClient openAIClient, LLMRequest request, CancellationToken cancellationToken);
    bool CanHandle(string model);
}
