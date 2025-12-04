using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Core.Extensions;

public static class ValidationExtensions
{
    public static (int StatusCode, LudusLogicaeResponse<TDto> Envelope)? ToBadRequestIfInvalid<TDto>(
        this ValidationResult validation)
    {
        if (validation.IsValid) return null;

        return ResponseFactory.BadRequest<TDto>(validation.Errors);
    }

    public static (int StatusCode, LudusLogicaeResponse<TDto> Envelope, Guid? CreatedId)?
        ToBadRequestWithIdIfInvalid<TDto>(
            this ValidationResult validation)
    {
        if (validation.IsValid) return null;

        return (400, ResponseFactory.BadRequest<TDto>(validation.Errors).Envelope, null);
    }
}
