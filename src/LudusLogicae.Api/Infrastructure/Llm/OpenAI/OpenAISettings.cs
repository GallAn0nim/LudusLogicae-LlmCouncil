using System.ComponentModel.DataAnnotations;

namespace LudusLogicae.Api.Infrastructure.Llm.OpenAI;

public sealed class OpenAISettings : IValidatableObject
{
    public const string SectionName = "OpenAI";

    private const int MinTimeoutSeconds = 10;
    private const int MaxTimeoutSeconds = 300;
    private const int MinRetryDelay = 50;
    private const int MaxRetryDelay = 5000;
    private const int MaxRetryCount = 10;
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    public string? OrganizationId { get; set; }
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 3;
    public int RetryBaseDelayMs { get; set; } = 250;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            yield return new ValidationResult(
                "BaseUrl is required.",
                [nameof(BaseUrl)]);
        }

        if (TimeoutSeconds < MinTimeoutSeconds || TimeoutSeconds > MaxTimeoutSeconds)
        {
            yield return new ValidationResult(
                $"TimeoutSeconds must be between {MinTimeoutSeconds} and {MaxTimeoutSeconds}.",
                [nameof(TimeoutSeconds)]);
        }

        if (MaxRetries is < 0 or > MaxRetryCount)
        {
            yield return new ValidationResult(
                $"MaxRetries must be between 0 and {MaxRetryCount}.",
                [nameof(MaxRetries)]);
        }

        if (RetryBaseDelayMs < MinRetryDelay || RetryBaseDelayMs > MaxRetryDelay)
        {
            yield return new ValidationResult(
                $"RetryBaseDelayMs must be between {MinRetryDelay} and {MaxRetryDelay}.",
                [nameof(RetryBaseDelayMs)]);
        }
    }
}
