namespace LudusLogicae.Api.Core.Common;

public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public List<string> Errors { get; init; } = new();

    public static ValidationResult Success()
    {
        return new ValidationResult();
    }

    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult { Errors = errors.ToList() };
    }

    public static ValidationResult Failure(string error)
    {
        return new ValidationResult { Errors = new List<string> { error } };
    }
}
