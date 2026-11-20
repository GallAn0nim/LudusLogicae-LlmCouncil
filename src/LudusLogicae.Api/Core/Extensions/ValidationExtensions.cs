using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Core.Extensions;

public static class ValidationExtensions
{
    public static ServiceResult<TDto>? ToBadRequestIfInvalid<TDto>(
        this ValidationResult validation)
    {
        if (validation.IsValid)
        {
            return null;
        }

        return ResponseFactory.BadRequest<TDto>(validation.Errors);
    }
}
