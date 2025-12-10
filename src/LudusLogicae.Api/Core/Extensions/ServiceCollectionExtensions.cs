using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;
using LudusLogicae.Api.Core.Configuration;
using LudusLogicae.Api.Features.Debates.Factories;
using LudusLogicae.Api.Features.Debates.Services;
using LudusLogicae.Api.Features.Debates.Services.Data;
using LudusLogicae.Api.Features.Debates.Services.Orchestration;
using LudusLogicae.Api.Features.Debates.Services.Orchestration.Phases;
using LudusLogicae.Api.Features.Debates.Services.Processing;
using LudusLogicae.Api.Features.Debates.Services.Results;
using LudusLogicae.Api.Features.Debates.Services.Rounds;
using LudusLogicae.Api.Features.Debates.Validation;
using LudusLogicae.Api.Features.Members.Services;
using LudusLogicae.Api.Features.Members.Services.Data;
using LudusLogicae.Api.Features.Members.Validation;
using LudusLogicae.Api.Features.Metadata.Services;
using LudusLogicae.Api.Infrastructure.Data;
using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Infrastructure.Llm.OpenAI;
using LudusLogicae.Api.Infrastructure.Llm.OpenAI.Strategies;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;
using LudusLogicae.Api.Infrastructure.Llm.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenAI;

namespace LudusLogicae.Api.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<OpenAISettings>()
            .Bind(configuration.GetSection(OpenAISettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<LlmProvidersOptions>()
            .Bind(configuration.GetSection(LlmProvidersOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<HealthCheckOptions>()
            .Bind(configuration.GetSection(HealthCheckOptions.SectionName));

        return services;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                                   ?? configuration["ConnectionStrings:Default"];

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            options.UseNpgsql(dataSource,
                npgsqlOptions => { npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public"); });
        });

        return services;
    }

    public static IServiceCollection AddLlmServices(this IServiceCollection services)
    {
        services.AddHttpClient("OpenAI", (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<OpenAISettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        services.AddSingleton<OpenAIClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<OpenAISettings>>().Value;
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            if (string.IsNullOrWhiteSpace(settings.ApiKey))
                throw new InvalidOperationException(
                    "OpenAI API key is required. Please set the OpenAI:ApiKey configuration value.");

            var httpClient = httpClientFactory.CreateClient("OpenAI");

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(settings.BaseUrl.TrimEnd('/') + "/"),
                Transport = new HttpClientPipelineTransport(httpClient)
            };

            return new OpenAIClient(
                new ApiKeyCredential(settings.ApiKey),
                options);
        });

        services.AddSingleton<IOpenAIModelStrategy, Gpt4ModelStrategy>();
        services.AddSingleton<IOpenAIModelStrategy, Gpt5ModelStrategy>();

        services.AddTransient<OpenAIResponsesClient>();
        services.AddTransient<ResilientOpenAIClient>();
        services.AddTransient<ILLMClient>(serviceProvider =>
            serviceProvider.GetRequiredService<ResilientOpenAIClient>());

        services.AddScoped<MemberPromptFactory>();
        services.AddScoped<DebatePromptFactory>();

        services.AddScoped<ILlmProviderService, LlmProviderService>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IMemberValidator, MemberValidator>();
        services.AddScoped<IDebateValidator, DebateValidator>();

        services.AddScoped<IDebateFactory, DebateFactory>();

        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<MemberIntroductionCacheRepository>();

        services.AddScoped<IMemberCrudService, MemberCrudService>();
        services.AddScoped<IMemberIntroductionService, MemberIntroductionService>();
        services.AddScoped<IMembersService, MembersService>();

        services.AddScoped<IMemberSelectionService, MemberSelectionService>();
        services.AddScoped<IMemberStatsUpdater, MemberStatsUpdater>();
        services.AddScoped<IMemberIntroductionFetcher, MemberIntroductionFetcher>();

        services.AddScoped<DebateParticipantFactory>();
        services.AddScoped<ILlmResponseGenerator, LlmResponseGenerator>();
        services.AddScoped<IDebateResponseParser, DebateResponseParser>();
        services.AddScoped<RoundPairingGenerator>();
        services.AddScoped<RoundTranscriptFormatter>();
        services.AddScoped<IDebateOrchestrator, DebateOrchestrator>();

        services.AddScoped<InitialAnswerPhaseExecutor>();
        services.AddScoped<RoundQuestionPhaseExecutor>();
        services.AddScoped<RoundAnswerPhaseExecutor>();
        services.AddScoped<AmendmentPhaseExecutor>();
        services.AddScoped<FinalStatementPhaseExecutor>();
        services.AddScoped<BallotPhaseExecutor>();
        services.AddScoped<TieBreakPhaseExecutor>();

        services.AddScoped<IDebateStepExecutor, DebateStepExecutor>();
        services.AddScoped<IDebateResultCalculator, DebateResultCalculator>();
        services.AddScoped<IDebateWinnerBuilder, DebateWinnerBuilder>();
        services.AddScoped<IDebateRepository, DebateRepository>();

        services.AddScoped<IDebatePhaseCoordinator, DebatePhaseCoordinator>();
        services.AddScoped<IDebateStateManager, DebateStateManager>();
        services.AddScoped<IDebateCompletionService, DebateCompletionService>();

        services.AddScoped<IDebateParticipantService, DebateParticipantService>();
        services.AddScoped<IDebateCrudService, DebateCrudService>();
        services.AddScoped<IDebateExecutionService, DebateExecutionService>();
        services.AddScoped<IDebateService, DebateService>();

        services.AddScoped<IMetadataService, MetadataService>();

        return services;
    }

    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        var connectionString = configuration.GetConnectionString("Default")
                               ?? configuration["ConnectionStrings:Default"];

        if (!string.IsNullOrWhiteSpace(connectionString))
            healthChecksBuilder.AddNpgSql(connectionString, name: "postgres");

        return services;
    }

    public static IMvcBuilder AddApiJsonOptions(this IMvcBuilder builder)
    {
        return builder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            options.JsonSerializerOptions.Converters.Add(new DateTimeOffsetJsonConverter());

            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        });
    }
}

public sealed class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.Parse(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("o"));
    }
}
