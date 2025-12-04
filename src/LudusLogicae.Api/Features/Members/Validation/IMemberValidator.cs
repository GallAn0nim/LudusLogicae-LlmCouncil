using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Features.Members.Contracts;

namespace LudusLogicae.Api.Features.Members.Validation;

public interface IMemberValidator
{
    ValidationResult ValidateCreateRequest(CreateMemberRequest request);
    ValidationResult ValidateUpdateRequest(UpdateMemberRequest request);
}
