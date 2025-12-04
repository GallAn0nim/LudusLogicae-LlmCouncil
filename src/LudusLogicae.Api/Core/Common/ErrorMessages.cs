namespace LudusLogicae.Api.Core.Common;

public static class ErrorMessages
{
    public static string NotFound(string entityName, Guid id)
    {
        return $"{entityName} with ID {id} was not found";
    }

    public static string NotFound(string entityName)
    {
        return $"{entityName} was not found";
    }


    public static string LlmError(string provider, string operation)
    {
        return $"LLM provider {provider} failed during {operation}";
    }

    public static string MissingRequired(string entityName, string context)
    {
        return $"{entityName} is required but {context}";
    }
}
