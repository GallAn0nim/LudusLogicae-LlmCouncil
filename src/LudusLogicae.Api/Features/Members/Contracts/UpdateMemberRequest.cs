using System.ComponentModel.DataAnnotations;
using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Members.Contracts;

public sealed record UpdateMemberRequest
{

    [Required]
    [StringLength(200)]
    public required string Name { get; init; }

    [Required]
    [StringLength(100)]
    public required string Role { get; init; }

    [Required]
    [StringLength(100)]
    public required string Provider { get; init; }

    [Required]
    [StringLength(100)]
    public required string Model { get; init; }

    [Range(0.0, 1.0)]
    public double Temperature { get; init; } = 0.5;

    [StringLength(200)]
    public string? StyleHint { get; init; }

    [StringLength(1000)]
    public string? BehaviorExtras { get; init; }

    [Required]
    public MemberStatus Status { get; init; } = MemberStatus.Active;
}
