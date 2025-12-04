using System.Text.Json.Serialization;

namespace LudusLogicae.Api.Features.Debates.Models;

public sealed record BallotOutput(
    [property: JsonPropertyName("ranks")] RankOutput[] Ranks);

public sealed record RankOutput(
    [property: JsonPropertyName("slot")] int Slot,
    [property: JsonPropertyName("rank")] int Rank);

public sealed record TieBreakOutput(
    [property: JsonPropertyName("vote")] int Vote);

public sealed record MemberDataForBallot(
    [property: JsonPropertyName("slot")] int Slot,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("profession")]
    string Profession,
    [property: JsonPropertyName("currentStance")]
    string CurrentStance,
    [property: JsonPropertyName("initialAnswer")]
    string InitialAnswer);
