namespace LudusLogicae.Api.Shared.Contracts;

public sealed record ServiceResult<T>(int StatusCode, LudusLogicaeResponse<T> Envelope, Guid? CreatedId = null);
