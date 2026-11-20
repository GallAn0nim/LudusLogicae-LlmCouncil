using System.ComponentModel.DataAnnotations;
using LudusLogicae.Api.Core.Configuration;
using LudusLogicae.Api.Core.Extensions;
using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Infrastructure.Llm.Anthropic;
using LudusLogicae.Api.Infrastructure.Llm.Models;
using LudusLogicae.Api.Infrastructure.Llm.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LudusLogicae.Api.Tests.Llm;

public class LlmProviderSelectionTests
{
    private static ServiceProvider Build(string? openAiKey, string? anthropicKey = null,
        Action<IServiceCollection>? extra = null)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Llm:DefaultProvider"] = "OpenAI",
            ["Llm:Providers:0:Name"] = "OpenAI",
            ["Llm:Providers:0:DefaultModel"] = "gpt-4.1-mini",
            ["Llm:Providers:0:SupportedModels:0"] = "gpt-4.1-nano",
            ["Llm:Providers:0:SupportedModels:1"] = "gpt-4.1-mini",
            ["Llm:Providers:1:Name"] = "Anthropic",
            ["Llm:Providers:1:DefaultModel"] = "claude-haiku-5-5",
            ["Llm:Providers:1:SupportedModels:0"] = "claude-haiku-5-5",
            ["OpenAI:ApiKey"] = openAiKey,
            ["Anthropic:ApiKey"] = anthropicKey
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationOptions(configuration);
        services.AddLlmServices(configuration);
        extra?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task WithoutApiKey_NoProviderAvailable_AndSendFailsCleanly()
    {
        using var sp = Build(null);
        using var scope = sp.CreateScope();
        var providers = scope.ServiceProvider.GetRequiredService<ILlmProviderService>();
        var client = scope.ServiceProvider.GetRequiredService<ILLMClient>();

        Assert.Empty(providers.GetSupportedProviders());
        Assert.Null(providers.GetDefaultProvider());
        Assert.False(providers.IsProviderSupported("OpenAI"));

        var ex = await Assert.ThrowsAsync<LLMClientException>(() =>
            client.SendAsync(new LLMRequest { Provider = "OpenAI" }));
        Assert.Contains("not configured", ex.Message);
        await Assert.ThrowsAsync<LLMClientException>(() => client.SendAsync(new LLMRequest()));
    }

    [Fact]
    public void WithApiKey_OpenAIAvailable_AndFallsBackToDefaults()
    {
        using var sp = Build("sk-test");
        using var scope = sp.CreateScope();
        var providers = scope.ServiceProvider.GetRequiredService<ILlmProviderService>();

        Assert.Equal(["OpenAI"], providers.GetSupportedProviders());
        Assert.Equal("OpenAI", providers.GetDefaultProvider());
        Assert.True(providers.IsProviderSupported("openai"));
        Assert.Equal("OpenAI", providers.ValidateAndFallbackProvider("Unknown"));
        Assert.Equal("gpt-4.1-mini", providers.ValidateAndFallbackModel("OpenAI", "gpt-99"));
        Assert.Equal("gpt-4.1-nano", providers.ValidateAndFallbackModel("OpenAI", "gpt-4.1-nano"));
        Assert.NotNull(scope.ServiceProvider.GetKeyedService<ILLMClient>(ProviderSelectingLLMClient.Key("OpenAI")));
    }

    [Fact]
    public void Options_RejectDefaultProviderOrModelNotListed()
    {
        var options = new LlmProvidersOptions
        {
            DefaultProvider = "Anthropic",
            Providers = [new LlmProviderConfig { Name = "OpenAI", DefaultModel = "x", SupportedModels = ["gpt-5"] }]
        };

        var errors = options.Validate(new ValidationContext(options)).Select(e => e.ErrorMessage).ToList();

        Assert.Contains(errors, e => e!.Contains("DefaultProvider"));
        Assert.Contains(errors, e => e!.Contains("DefaultModel"));
    }

    [Fact]
    public void WithAnthropicKey_AnthropicRegistered_AndOpenAIFallsBackToIt()
    {
        using var sp = Build(null, "sk-ant-test");
        using var scope = sp.CreateScope();
        var providers = scope.ServiceProvider.GetRequiredService<ILlmProviderService>();

        Assert.Equal(["Anthropic"], providers.GetSupportedProviders());
        Assert.Equal("Anthropic", providers.ValidateAndFallbackProvider("OpenAI"));
        Assert.Equal("claude-haiku-5-5", providers.ValidateAndFallbackModel("Anthropic", "gpt-4.1-nano"));
        Assert.IsType<AnthropicLLMClient>(
            scope.ServiceProvider.GetKeyedService<ILLMClient>(ProviderSelectingLLMClient.Key("Anthropic")));
    }

    [Fact]
    public async Task Router_SendsUnavailableProviderRequestToDefaultWithDefaultModel()
    {
        var fake = new RecordingClient();
        using var sp = Build(null, extra: s =>
        {
            s.AddKeyedSingleton<ILLMClient>(ProviderSelectingLLMClient.Key("Anthropic"), fake);
            s.Configure<LlmProvidersOptions>(o => o.DefaultProvider = "Anthropic");
        });
        using var scope = sp.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ILLMClient>()
            .SendAsync(new LLMRequest { Provider = "OpenAI", Model = "gpt-4.1-nano" });

        Assert.Equal("Anthropic", fake.Last!.Provider);
        Assert.Equal("claude-haiku-5-5", fake.Last.Model);
    }

    private sealed class RecordingClient : ILLMClient
    {
        public LLMRequest? Last { get; private set; }

        public Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken cancellationToken = default)
        {
            Last = request;
            return Task.FromResult(new LLMResponse { Text = "ok" });
        }
    }
}
