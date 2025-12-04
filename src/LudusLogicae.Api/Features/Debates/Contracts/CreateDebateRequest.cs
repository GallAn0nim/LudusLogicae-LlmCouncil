using LudusLogicae.Api.Core.Common.Constants;

namespace LudusLogicae.Api.Features.Debates.Contracts;

public sealed record CreateDebateRequest
{
    public required string Question { get; init; }

    public int NMembers { get; init; } = DebateConstants.DefaultMemberCount;

    public int NRounds { get; init; } = DebateConstants.DefaultRoundsCount;

    public string? Context { get; init; }

    public List<Guid>? MemberIds { get; init; }
}
