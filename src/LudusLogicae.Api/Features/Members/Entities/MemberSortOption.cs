using System.Text.Json.Serialization;

namespace LudusLogicae.Api.Features.Members.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemberSortOption
{

    CreatedDesc,
    CreatedAsc,
    PointsDesc,
    PointsAsc,
    WinsDesc,
    WinsAsc,
    DebatesDesc,
    DebatesAsc,
    NameAsc,
    NameDesc
}
