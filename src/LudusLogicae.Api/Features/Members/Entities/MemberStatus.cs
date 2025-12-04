using System.Text.Json.Serialization;

namespace LudusLogicae.Api.Features.Members.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemberStatus
{
    Active = 0,
    Benched = 1,
    Retired = 2
}
