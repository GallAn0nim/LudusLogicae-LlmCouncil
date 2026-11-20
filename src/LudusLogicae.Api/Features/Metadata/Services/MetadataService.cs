using System.Reflection;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Members.Services;
using LudusLogicae.Api.Features.Metadata.Contracts;
using LudusLogicae.Api.Infrastructure.Llm.Services;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Metadata.Services;

public sealed class MetadataService(ILlmProviderService llmProviderService) : IMetadataService
{
    public Task<ServiceResult<string>> PingAsync(CancellationToken ct)
    {
        return Task.FromResult(ResponseFactory.Ok("pong"));
    }

    public Task<ServiceResult<MetadataInfoResponse>> InfoAsync(CancellationToken ct)
    {
        var info = BuildInfo();
        return Task.FromResult(ResponseFactory.Ok(info));
    }

    public Task<ServiceResult<List<string>>> GetProvidersAsync(CancellationToken ct)
    {
        var names = GetConfiguredProviders();
        return Task.FromResult(ResponseFactory.Ok(names));
    }

    public Task<ServiceResult<List<string>>> GetProviderModelsAsync(string id,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Task.FromResult(ResponseFactory.BadRequest<List<string>>("Provider id is required."));
        }

        var models = GetModelsForProvider(id);
        if (models.Count == 0 &&
            !GetConfiguredProviders().Any(p => string.Equals(p, id, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(ResponseFactory.NotFound<List<string>>($"Provider '{id}' not found."));
        }

        return Task.FromResult(ResponseFactory.Ok(models));
    }

    public Task<ServiceResult<List<string>>> GetDebatePhaseesAsync(
        CancellationToken ct)
    {
        var values = Enum.GetNames(typeof(DebatePhase)).ToList();
        return Task.FromResult(ResponseFactory.Ok(values));
    }

    public Task<ServiceResult<Dictionary<string, string>>> GetMemberSortOptionsAsync(
        CancellationToken ct)
    {
        var sortOptions = MemberSortHelper.GetSortOptions();
        return Task.FromResult(ResponseFactory.Ok(sortOptions));
    }

    private MetadataInfoResponse BuildInfo()
    {
        var entry = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var informational = entry.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var productVersion = entry.GetName().Version?.ToString();
        var version = informational ?? productVersion ?? "0.0.0";

        return new MetadataInfoResponse
        {
            Name = "Ludus Logicae",
            Version = version,
            Description =
                "AI debate platform where multiple LLM-powered members engage in structured debates with transparent scoring and performance tracking.",
            Goals =
            [
                "Enable multi-agent debate sessions with configurable members",
                "Track member performance across debates (wins, points, participation)",
                "Provide transparent voting and scoring using Borda count",
                "Support debate orchestration with configurable rounds and phases"
            ],
            DebateFlow =
            [
                "Created - Debate initialized with question, context, number of members and rounds",
                "InitialAnswer - Each member provides their initial position on the question",
                "CrossQuestion - Members ask each other questions (repeats for N rounds)",
                "Response - Members respond to questions directed at them",
                "Amendment - Members can amend their initial positions based on discussion",
                "FinalAnswer - Each member presents their final position",
                "Ballot - Members vote using ranked ballots (Borda count)",
                "Completed - Winner determined, results and full state persisted",
                "Failed - Debate execution encountered an error"
            ],
            Principles =
            [
                "Feature-based architecture for maintainability",
                "Clean separation of concerns (validation, orchestration, processing)",
                "Explicit state management with full debate state persistence as JSONB",
                "Member-centric design with stats tracking and introduction caching",
                "Provider-agnostic LLM integration with fallback support"
            ],
            Stack = new StackInfo
            {
                Backend = ".NET 11 Web API with EF Core 11",
                Data = "PostgreSQL with JSONB for state/results storage",
                Deployment = "Docker + docker-compose (API + PostgreSQL)"
            }
        };
    }

    private List<string> GetConfiguredProviders()
    {
        return llmProviderService.GetSupportedProviders()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private List<string> GetModelsForProvider(string provider)
    {
        return llmProviderService.GetSupportedModelsForProvider(provider).ToList();
    }
}
