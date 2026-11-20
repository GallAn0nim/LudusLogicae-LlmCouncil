using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Validation;
using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Features.Members.Validation;

namespace LudusLogicae.Api.Tests.Validation;

public class DebateValidatorTests
{
    private readonly DebateValidator _validator = new();

    [Theory]
    [InlineData(DebateConstants.MinimumMemberCount)]
    [InlineData(DebateConstants.DefaultMemberCount)]
    [InlineData(DebateConstants.MaximumMemberCount)]
    public void Member_count_within_limits_is_valid(int count)
    {
        Assert.True(_validator.ValidateDebateConfiguration(count).IsValid);
    }

    [Theory]
    [InlineData(DebateConstants.MinimumMemberCount - 1, "at least 3")]
    [InlineData(0, "at least 3")]
    [InlineData(DebateConstants.MaximumMemberCount + 1, "cannot exceed 7")]
    public void Member_count_outside_limits_is_rejected(int count, string message)
    {
        var result = _validator.ValidateDebateConfiguration(count);

        Assert.False(result.IsValid);
        Assert.Contains(message, Assert.Single(result.Errors));
    }

    [Theory]
    [InlineData(DebatePhase.Created, true)]
    [InlineData(DebatePhase.InitialAnswer, false)]
    [InlineData(DebatePhase.CrossQuestion, false)]
    [InlineData(DebatePhase.Response, false)]
    [InlineData(DebatePhase.Amendment, false)]
    [InlineData(DebatePhase.FinalAnswer, false)]
    [InlineData(DebatePhase.Ballot, false)]
    [InlineData(DebatePhase.Completed, false)]
    [InlineData(DebatePhase.Failed, false)]
    public void Only_created_debates_are_editable(DebatePhase phase, bool editable)
    {
        Assert.Equal(editable, _validator.IsDebateEditable(new Debate { Phase = phase }));
    }

    [Fact]
    public void Every_phase_is_covered_by_the_editability_test()
    {
        Assert.Equal(9, Enum.GetValues<DebatePhase>().Length);
    }

    [Theory]
    [InlineData(DebateOperationType.Update)]
    [InlineData(DebateOperationType.Delete)]
    [InlineData(DebateOperationType.Start)]
    public void Editability_check_passes_for_created_debate(DebateOperationType operation)
    {
        Assert.Null(DebateValidationHelper.ValidateEditability<object>(
            new Debate { Phase = DebatePhase.Created }, _validator, operation));
    }

    [Theory]
    [InlineData(DebateOperationType.Update, "updated")]
    [InlineData(DebateOperationType.Delete, "deleted")]
    [InlineData(DebateOperationType.Start, "started")]
    public void Editability_check_returns_400_for_started_debate(DebateOperationType operation, string verb)
    {
        var error = DebateValidationHelper.ValidateEditability<object>(
            new Debate { Phase = DebatePhase.Completed }, _validator, operation);

        Assert.NotNull(error);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("400", error.Envelope.Metadata.StatusCode);
        Assert.Equal($"Only debates in 'Created' status can be {verb}.", Assert.Single(error.Envelope.Metadata.Errors));
    }
}

public class MemberValidatorTests
{
    private readonly MemberValidator _validator = new();

    private static CreateMemberRequest Create(double temperature)
    {
        return new()
        {
            Name = "Test",
            Role = "Tester",
            Provider = "OpenAI",
            Model = "gpt-4.1-mini",
            Temperature = temperature
        };
    }

    private static UpdateMemberRequest Update(double temperature)
    {
        return new()
        {
            Name = "Test",
            Role = "Tester",
            Provider = "OpenAI",
            Model = "gpt-4.1-mini",
            Temperature = temperature
        };
    }

    [Theory]
    [InlineData(MemberConstants.MinTemperature)]
    [InlineData(MemberConstants.DefaultTemperature)]
    [InlineData(MemberConstants.MaxTemperature)]
    public void Temperature_within_range_is_valid(double temperature)
    {
        Assert.True(_validator.ValidateCreateRequest(Create(temperature)).IsValid);
        Assert.True(_validator.ValidateUpdateRequest(Update(temperature)).IsValid);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(2.0)]
    [InlineData(double.NegativeInfinity)]
    public void Temperature_outside_range_is_rejected(double temperature)
    {
        var create = _validator.ValidateCreateRequest(Create(temperature));
        var update = _validator.ValidateUpdateRequest(Update(temperature));

        Assert.False(create.IsValid);
        Assert.False(update.IsValid);
        Assert.Equal("Temperature must be between 0 and 1.", Assert.Single(create.Errors));
    }

    [Fact]
    public void Temperature_NaN_is_rejected()
    {
        Assert.False(_validator.ValidateCreateRequest(Create(double.NaN)).IsValid);
    }
}
