using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Core.Extensions;
using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Features.Members.Services.Data;
using LudusLogicae.Api.Infrastructure.Llm.Abstractions;
using LudusLogicae.Api.Infrastructure.Llm.Prompts;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Members.Services;

public sealed class MemberIntroductionService(
    IMemberRepository repository,
    MemberIntroductionCacheRepository cacheRepository,
    ILLMClient llmClient,
    MemberPromptFactory promptFactory,
    ILogger<MemberIntroductionService> logger) : IMemberIntroductionService
{
    public async Task<ServiceResult<MemberIntroductionResponse>>
        GenerateIntroductionAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        var member = await repository.GetByIdAsync(id, cancellationToken);

        if (member is null)
        {
            return member.ToNotFoundResponse<Member, MemberIntroductionResponse>("Member", id);
        }

        try
        {
            var cached = await cacheRepository.GetCachedIntroductionAsync(member, cancellationToken);
            if (cached is not null)
            {
                var cachedResponse = new MemberIntroductionResponse
                {
                    MemberId = member.Id,
                    Provider = member.Provider,
                    Model = member.Model,
                    Temperature = member.Temperature,
                    Introduction = cached.Introduction
                };
                return ResponseFactory.Ok(cachedResponse, "OK (cached)");
            }

            var freshIntroduction = await GenerateIntroductionFromLlmAsync(member, cancellationToken);

            if (IsFromMemberModel(member, freshIntroduction))
            {
                await cacheRepository.SaveOrUpdateAsync(member, freshIntroduction.Introduction, cancellationToken);
            }

            return ResponseFactory.Ok(freshIntroduction);
        }
        catch (LLMClientException ex)
        {
            logger.LogError(ex, "LLM provider error while generating introduction for member {MemberId}", id);
            var errorMessage = ErrorMessages.LlmError(ex.Provider, "introduction generation");
            return ResponseFactory.BadGateway<MemberIntroductionResponse>(errorMessage, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while generating introduction for member {MemberId}", id);
            return ResponseFactory.InternalServerError<MemberIntroductionResponse>(
                "Unexpected error while generating introduction.", ex);
        }
    }

    private static bool IsFromMemberModel(Member member, MemberIntroductionResponse introduction)
    {
        return string.Equals(introduction.Provider, member.Provider, StringComparison.OrdinalIgnoreCase)
               && string.Equals(introduction.Model, member.Model, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<MemberIntroductionResponse> GenerateIntroductionFromLlmAsync(
        Member member,
        CancellationToken cancellationToken)
    {
        var request = promptFactory.BuildIntroductionPrompt(member);
        var llmResponse = await llmClient.SendAsync(request, cancellationToken);

        return new MemberIntroductionResponse
        {
            MemberId = member.Id,
            Provider = llmResponse.Provider,
            Model = string.IsNullOrWhiteSpace(llmResponse.Model) ? member.Model : llmResponse.Model,
            Temperature = member.Temperature,
            Introduction = (llmResponse.Text ?? string.Empty).Replace("\\n", "\n")
        };
    }
}
