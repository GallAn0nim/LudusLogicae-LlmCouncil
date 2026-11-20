using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Members.Contracts;

namespace LudusLogicae.Api.Features.Members.Validation;

public sealed class MemberValidator : IMemberValidator
{
    public ValidationResult ValidateCreateRequest(CreateMemberRequest request)
    {
        return ValidateBusinessRules(request.Temperature);
    }

    public ValidationResult ValidateUpdateRequest(UpdateMemberRequest request)
    {
        return ValidateBusinessRules(request.Temperature);
    }

    private static ValidationResult ValidateBusinessRules(double temperature)
    {
        if (!(temperature >= MemberConstants.MinTemperature && temperature <= MemberConstants.MaxTemperature))
        {
            return ValidationResult.Failure(
                $"Temperature must be between {MemberConstants.MinTemperature} and {MemberConstants.MaxTemperature}.");
        }

        return ValidationResult.Success();
    }
}
