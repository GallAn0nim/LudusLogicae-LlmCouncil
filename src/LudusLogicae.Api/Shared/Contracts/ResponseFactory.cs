namespace LudusLogicae.Api.Shared.Contracts;

public static class ResponseFactory
{
    public static ServiceResult<T> Ok<T>(T data, string description = "OK")
    {
        return new ServiceResult<T>(200, new LudusLogicaeResponse<T>
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

    public static ServiceResult<T> Created<T>(T data, string description = "Created")
    {
        return new ServiceResult<T>(201, new LudusLogicaeResponse<T>
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

    public static ServiceResult<T> NoContent<T>()
    {
        return new ServiceResult<T>(204, new LudusLogicaeResponse<T>
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

    public static ServiceResult<T> NotFound<T>(string message)
    {
        return CreateErrorResponse<T>(404, "Not Found", message);
    }

    public static ServiceResult<T> BadRequest<T>(IEnumerable<string> errors)
    {
        return CreateErrorResponse<T>(400, "Bad Request", errors);
    }

    public static ServiceResult<T> BadRequest<T>(string error)
    {
        return BadRequest<T>([error]);
    }

    public static ServiceResult<T> InternalServerError<T>(string message,
        Exception? ex = null)
    {
        return CreateErrorResponse<T>(500, "Internal Server Error", message, ex);
    }

    public static ServiceResult<T> BadGateway<T>(string message, Exception? ex = null)
    {
        return CreateErrorResponse<T>(502, "Bad Gateway", message, ex);
    }

    private static ServiceResult<T> CreateErrorResponse<T>(
        int statusCode,
        string statusDescription,
        string message,
        Exception? exception = null)
    {
        var errors = new List<string> { message };
        if (exception != null)
        {
            errors.Add(exception.Message);
        }

        return new ServiceResult<T>(statusCode, new LudusLogicaeResponse<T>
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

    private static ServiceResult<T> CreateErrorResponse<T>(
        int statusCode,
        string statusDescription,
        IEnumerable<string> errors)
    {
        return new ServiceResult<T>(statusCode, new LudusLogicaeResponse<T>
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
