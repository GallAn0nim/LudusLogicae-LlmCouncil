namespace LudusLogicae.Api.Features.Debates.Contracts;

public sealed record UpdateDebateRequest
{
    public string? Question { get; init; }

    public int? NMembers { get; init; }

    public int? NRounds { get; init; }

    public string? Context { get; init; }

    public List<Guid>? MemberIds { get; init; }
}
