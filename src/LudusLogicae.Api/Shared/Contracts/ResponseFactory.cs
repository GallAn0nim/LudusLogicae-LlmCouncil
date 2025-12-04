namespace LudusLogicae.Api.Shared.Contracts;

public static class ResponseFactory
{
    public static (int StatusCode, LudusLogicaeResponse<T> Envelope) Ok<T>(T data, string description = "OK")
    {
        return (200, new LudusLogicaeResponse<T>
        {
            Data = data,
            Metadata = new LudusLogicaeResponseMetadata
            {
                StatusCode = "200",
                StatusDescription = description,
                Errors = []
            }
        });
    }

    public static (int StatusCode, LudusLogicaeResponse<T> Envelope) Created<T>(T data, string description = "Created")
    {
        return (201, new LudusLogicaeResponse<T>
        {
            Data = data,
            Metadata = new LudusLogicaeResponseMetadata
            {
                StatusCode = "201",
                StatusDescription = description,
                Errors = []
            }
        });
    }

    public static (int StatusCode, LudusLogicaeResponse<T> Envelope) NoContent<T>()
    {
        return (204, new LudusLogicaeResponse<T>
        {
            Data = default!,
            Metadata = new LudusLogicaeResponseMetadata
            {
                StatusCode = "204",
                StatusDescription = "No Content",
                Errors = []
            }
        });
    }

    public static (int StatusCode, LudusLogicaeResponse<T> Envelope) NotFound<T>(string message)
    {
        return CreateErrorResponse<T>(404, "Not Found", message);
    }

    public static (int StatusCode, LudusLogicaeResponse<T> Envelope) BadRequest<T>(IEnumerable<string> errors)
    {
        return CreateErrorResponse<T>(400, "Bad Request", errors);
    }

    public static (int StatusCode, LudusLogicaeResponse<T> Envelope) BadRequest<T>(string error)
    {
        return BadRequest<T>([error]);
    }

    public static (int StatusCode, LudusLogicaeResponse<T> Envelope) InternalServerError<T>(string message,
        Exception? ex = null)
    {
        return CreateErrorResponse<T>(500, "Internal Server Error", message, ex);
    }

    public static (int StatusCode, LudusLogicaeResponse<T> Envelope) BadGateway<T>(string message, Exception? ex = null)
    {
        return CreateErrorResponse<T>(502, "Bad Gateway", message, ex);
    }

    private static (int StatusCode, LudusLogicaeResponse<T> Envelope) CreateErrorResponse<T>(
        int statusCode,
        string statusDescription,
        string message,
        Exception? exception = null)
    {
        var errors = new List<string> { message };
        if (exception != null) errors.Add(exception.Message);

        return (statusCode, new LudusLogicaeResponse<T>
        {
            Data = default!,
            Metadata = new LudusLogicaeResponseMetadata
            {
                StatusCode = statusCode.ToString(),
                StatusDescription = statusDescription,
                Errors = errors.ToArray()
            }
        });
    }

    private static (int StatusCode, LudusLogicaeResponse<T> Envelope) CreateErrorResponse<T>(
        int statusCode,
        string statusDescription,
        IEnumerable<string> errors)
    {
        return (statusCode, new LudusLogicaeResponse<T>
        {
            Data = default!,
            Metadata = new LudusLogicaeResponseMetadata
            {
                StatusCode = statusCode.ToString(),
                StatusDescription = statusDescription,
                Errors = errors.ToArray()
            }
        });
    }
}
