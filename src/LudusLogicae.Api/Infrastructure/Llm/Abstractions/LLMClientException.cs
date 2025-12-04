using System.Net;

namespace LudusLogicae.Api.Infrastructure.Llm.Abstractions;

public class LLMClientException : Exception
{
    public LLMClientException(string message, string provider, string? providerErrorType = null,
        HttpStatusCode? statusCode = null, Exception? inner = null)
        : base(message, inner)
    {
        Provider = provider;
        ProviderErrorType = providerErrorType;
        StatusCode = statusCode;
    }

    public string Provider { get; }
    public string? ProviderErrorType { get; }
    public HttpStatusCode? StatusCode { get; }
}
