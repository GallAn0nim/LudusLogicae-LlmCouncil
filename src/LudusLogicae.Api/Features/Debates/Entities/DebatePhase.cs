using System.Text.Json.Serialization;

namespace LudusLogicae.Api.Features.Debates.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DebatePhase
{
    Created = 0,
    InitialAnswer = 1,
    CrossQuestion = 2,
    Response = 3,
    Amendment = 4,
    FinalAnswer = 5,
    Ballot = 6,
    Completed = 7,
    Failed = 8
}
