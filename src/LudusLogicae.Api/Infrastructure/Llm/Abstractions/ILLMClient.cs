using LudusLogicae.Api.Infrastructure.Llm.Models;

namespace LudusLogicae.Api.Infrastructure.Llm.Abstractions;

public interface ILLMClient
{
    Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken cancellationToken = default);
}
