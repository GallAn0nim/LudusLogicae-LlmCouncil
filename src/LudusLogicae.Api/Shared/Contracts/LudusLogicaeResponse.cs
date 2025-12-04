namespace LudusLogicae.Api.Shared.Contracts;

public sealed class LudusLogicaeResponse<T>
{
    public LudusLogicaeResponse()
    {
        Data = default!;
        Metadata = new LudusLogicaeResponseMetadata();
    }

    public LudusLogicaeResponse(T data, LudusLogicaeResponseMetadata metadata)
    {
        Data = data;
        Metadata = metadata;
    }

    public T Data { get; init; }

    public LudusLogicaeResponseMetadata Metadata { get; init; }
}
