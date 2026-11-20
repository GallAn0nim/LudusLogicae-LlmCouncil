using System.ComponentModel.DataAnnotations;

namespace LudusLogicae.Api.Core.Configuration;

public sealed class DatabaseOptions : IValidatableObject
{
    public const string SectionName = "Database";

    public bool AutoMigrate { get; set; } = true;

    public int MigrationRetrySeconds { get; set; } = 60;

    public int RetryBaseDelayMs { get; set; } = 500;

    public int RetryMaxDelayMs { get; set; } = 5000;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MigrationRetrySeconds < 0)
        {
            yield return new ValidationResult(
                "MigrationRetrySeconds must be non-negative.",
                [nameof(MigrationRetrySeconds)]);
        }

        if (MigrationRetrySeconds > 600)
        {
            yield return new ValidationResult(
                "MigrationRetrySeconds should not exceed 600 seconds (10 minutes).",
                [nameof(MigrationRetrySeconds)]);
        }

        if (RetryBaseDelayMs < 100 || RetryBaseDelayMs > 10000)
        {
            yield return new ValidationResult(
                "RetryBaseDelayMs must be between 100 and 10000 milliseconds.",
                [nameof(RetryBaseDelayMs)]);
        }

        if (RetryMaxDelayMs < 1000 || RetryMaxDelayMs > 60000)
        {
            yield return new ValidationResult(
                "RetryMaxDelayMs must be between 1000 and 60000 milliseconds.",
                [nameof(RetryMaxDelayMs)]);
        }
    }
}
