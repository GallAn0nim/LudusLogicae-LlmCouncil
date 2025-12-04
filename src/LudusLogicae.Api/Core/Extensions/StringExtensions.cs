namespace LudusLogicae.Api.Core.Extensions;

public static class StringExtensions
{
    public static string TrimOrEmpty(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
