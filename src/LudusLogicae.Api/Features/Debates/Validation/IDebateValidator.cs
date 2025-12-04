using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Features.Debates.Entities;

namespace LudusLogicae.Api.Features.Debates.Validation;

public interface IDebateValidator
{
    ValidationResult ValidateDebateConfiguration(int memberCount);
    bool IsDebateEditable(Debate debate);
}
