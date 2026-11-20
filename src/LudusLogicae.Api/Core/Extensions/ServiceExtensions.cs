using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Core.Extensions;

public static class ServiceExtensions
{
    public static ServiceResult<TDto> ToNotFoundResponse<TEntity, TDto>(
        this TEntity? entity,
        string entityName,
        Guid? id = null)
        where TEntity : class
    {
        if (entity is not null)
        {
            throw new InvalidOperationException(
                "ToNotFoundResponse should only be called when entity is null. Use ToOkResponse instead.");
        }

        var message = id.HasValue
            ? ErrorMessages.NotFound(entityName, id.Value)
            : ErrorMessages.NotFound(entityName);

        return ResponseFactory.NotFound<TDto>(message);
    }

    public static ServiceResult<TDto> ToOkResponse<TEntity, TDto>(
        this TEntity entity,
        Func<TEntity, TDto> mapper)
    {
        return ResponseFactory.Ok(mapper(entity));
    }

    public static ServiceResult<TDto> ToOkResponse<TDto>(this TDto dto)
    {
        return ResponseFactory.Ok(dto);
    }

    public static ServiceResult<TDto> ToBadRequestResponse<TDto>(
        this IEnumerable<string> errors)
    {
        return ResponseFactory.BadRequest<TDto>(errors);
    }

    public static ServiceResult<TDto> ToBadRequestResponse<TDto>(
        this string errorMessage)
    {
        return ResponseFactory.BadRequest<TDto>(errorMessage);
    }
}
