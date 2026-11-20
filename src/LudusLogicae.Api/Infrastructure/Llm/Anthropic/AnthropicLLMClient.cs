using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Infrastructure.Llm.Models;

namespace LudusLogicae.Api.Infrastructure.Llm.Anthropic;

public sealed class AnthropicLLMClient : ILLMClient
{
    public const string ProviderName = "Anthropic";
    private const int DefaultMaxTokens = 16000;

    private const int ThinkingHeadroomTokens = 4000;

    private readonly AnthropicClient _client;
    private readonly ILogger<AnthropicLLMClient> _logger;

    public AnthropicLLMClient(AnthropicClient client, ILogger<AnthropicLLMClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var model = request.Model ?? string.Empty;

        var system = string.Join("\n\n", new[] { request.SystemPrompt }
            .Concat(request.Messages.Where(m => m.Role == LLMRole.System).Select(m => m.Content))
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        var parameters = new MessageCreateParams
        {
            Model = model,
            MaxTokens = (request.MaxOutputTokens ?? DefaultMaxTokens) + ThinkingHeadroomTokens,
            OutputConfig = new OutputConfig { Effort = Effort.Medium },
            System = string.IsNullOrWhiteSpace(system) ? null : system,
            Messages = request.Messages
                .Where(m => m.Role != LLMRole.System)
                .Select(m => m.Role == LLMRole.Assistant
                    ? (MessageParam)new AssistantMessageParam { Content = m.Content }
                    : new UserMessageParam { Content = m.Content })
                .ToList()
        };

        Message response;
        try
        {
            response = await _client.Messages.Create(parameters, cancellationToken);
        }
        catch (AnthropicIOException ex)
        {
            _logger.LogError(ex, "Network error calling Anthropic model {Model}", model);
            throw new LLMClientException($"Could not reach Anthropic for model {model}.", ProviderName, inner: ex);
        }
        catch (AnthropicApiException ex)
        {
            _logger.LogError(ex, "Anthropic API error for model {Model}", model);
            throw new LLMClientException($"Anthropic request failed for model {model}: {ex.Message}", ProviderName,
                ex.GetType().Name, inner: ex);
        }

        if (response.StopReason == "refusal")
        {
            throw new LLMClientException($"Anthropic model {model} declined the request.", ProviderName, "refusal");
        }

        var text = string.Concat(response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
        if (response.StopReason == "max_tokens")
        {
            _logger.LogWarning("Anthropic response hit max_tokens ({MaxTokens}) for model {Model}",
                parameters.MaxTokens, model);
        }

        return new LLMResponse
        {
            Provider = ProviderName,
            Model = model,
            Text = text,
            Usage = new LLMUsage
            {
                PromptTokens = (int)response.Usage.InputTokens,
                CompletionTokens = (int)response.Usage.OutputTokens,
                TotalTokens = (int)(response.Usage.InputTokens + response.Usage.OutputTokens)
            },
            FinishReason = response.StopReason?.ToString(),
            ProviderCreatedAt = DateTimeOffset.UtcNow
        };
    }
}
