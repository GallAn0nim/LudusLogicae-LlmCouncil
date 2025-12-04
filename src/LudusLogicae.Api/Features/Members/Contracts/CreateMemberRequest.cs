using System.ComponentModel.DataAnnotations;
using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Members.Contracts;

public sealed record CreateMemberRequest
{
    [Required]
    [StringLength(MemberConstants.MaxNameLength)]
    public required string Name { get; init; }

    [Required]
    [StringLength(MemberConstants.MaxRoleLength)]
    public required string Role { get; init; }

    [Required]
    [StringLength(MemberConstants.MaxProviderLength)]
    public required string Provider { get; init; }

    [Required]
    [StringLength(MemberConstants.MaxModelLength)]
    public required string Model { get; init; }

    [Range(MemberConstants.MinTemperature, MemberConstants.MaxTemperature)]
    public double Temperature { get; init; } = MemberConstants.DefaultTemperature;

    [StringLength(MemberConstants.MaxStyleHintLength)]
    public string? StyleHint { get; init; }

    [StringLength(MemberConstants.MaxBehaviorExtrasLength)]
    public string? BehaviorExtras { get; init; }

    [Required]
    public MemberStatus Status { get; init; } = MemberStatus.Active;
}
