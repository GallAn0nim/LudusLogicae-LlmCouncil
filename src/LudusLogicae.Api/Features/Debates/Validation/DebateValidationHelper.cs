using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Shared.Contracts;

namespace LudusLogicae.Api.Features.Debates.Validation;

public enum DebateOperationType
{
    Update,
    Delete,
    Start
}

public static class DebateValidationHelper
{
    public static (int StatusCode, LudusLogicaeResponse<T> Envelope)? ValidateEditability<T>(
         Debate debate,
         IDebateValidator validator,
         DebateOperationType operation)
    {
        if (!validator.IsDebateEditable(debate))
        {
            var action = operation switch
            {
                DebateOperationType.Update => "updated",
                DebateOperationType.Delete => "deleted",
                DebateOperationType.Start => "started",
                _ => "modified"
            };

            return ResponseFactory.BadRequest<T>($"Only debates in 'Created' status can be {action}.");
        }

        return null;
    }
}
