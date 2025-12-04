using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;

namespace LudusLogicae.Api.Features.Debates.Validation;

public sealed class DebateValidator : IDebateValidator
{
    public ValidationResult ValidateDebateConfiguration(int memberCount)
    {
        return memberCount switch
        {
            < DebateConstants.MinimumMemberCount => ValidationResult.Failure(
                $"Member count must be at least {DebateConstants.MinimumMemberCount}."),
            > DebateConstants.MaximumMemberCount => ValidationResult.Failure(
                $"Member count cannot exceed {DebateConstants.MaximumMemberCount}."),
            _ => ValidationResult.Success()
        };
    }
    public bool IsDebateEditable(Debate debate)
    {
        return debate.Phase == DebatePhase.Created;
    }
}
