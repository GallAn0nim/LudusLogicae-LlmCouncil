using LudusLogicae.Api.Features.Members.Entities;

namespace LudusLogicae.Api.Features.Members.Services;

public static class MemberSortHelper
{
    public static MemberSortOption ParseSortOption(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return MemberSortOption.CreatedDesc;

        if (Enum.TryParse<MemberSortOption>(sortBy, true, out var enumValue)) return enumValue;

        return sortBy.ToLowerInvariant() switch
        {
            "points" => MemberSortOption.PointsDesc,
            "points_asc" => MemberSortOption.PointsAsc,
            "wins" => MemberSortOption.WinsDesc,
            "wins_asc" => MemberSortOption.WinsAsc,
            "debates" => MemberSortOption.DebatesDesc,
            "debates_asc" => MemberSortOption.DebatesAsc,
            "name" => MemberSortOption.NameAsc,
            "name_desc" => MemberSortOption.NameDesc,
            "created" => MemberSortOption.CreatedDesc,
            "created_asc" => MemberSortOption.CreatedAsc,
            _ => MemberSortOption.CreatedDesc
        };
    }

    public static IQueryable<Member> ApplySorting(IQueryable<Member> query, MemberSortOption sortOption)
    {
        return sortOption switch
        {
            MemberSortOption.PointsDesc => query.OrderByDescending(m => m.Points),
            MemberSortOption.PointsAsc => query.OrderBy(m => m.Points),
            MemberSortOption.WinsDesc => query.OrderByDescending(m => m.WinsCount),
            MemberSortOption.WinsAsc => query.OrderBy(m => m.WinsCount),
            MemberSortOption.DebatesDesc => query.OrderByDescending(m => m.DebatesCount),
            MemberSortOption.DebatesAsc => query.OrderBy(m => m.DebatesCount),
            MemberSortOption.NameAsc => query.OrderBy(m => m.Name),
            MemberSortOption.NameDesc => query.OrderByDescending(m => m.Name),
            MemberSortOption.CreatedAsc => query.OrderBy(m => m.CreatedAt),
            MemberSortOption.CreatedDesc => query.OrderByDescending(m => m.CreatedAt),
            _ => query.OrderByDescending(m => m.CreatedAt)
        };
    }

    public static Dictionary<string, string> GetSortOptions()
    {
        return new Dictionary<string, string>
        {
            [nameof(MemberSortOption.CreatedDesc)] = "Creation date (newest first) - default",
            [nameof(MemberSortOption.CreatedAsc)] = "Creation date (oldest first)",
            [nameof(MemberSortOption.PointsDesc)] = "Points (highest first)",
            [nameof(MemberSortOption.PointsAsc)] = "Points (lowest first)",
            [nameof(MemberSortOption.WinsDesc)] = "Wins count (most wins first)",
            [nameof(MemberSortOption.WinsAsc)] = "Wins count (fewest wins first)",
            [nameof(MemberSortOption.DebatesDesc)] = "Debates count (most debates first)",
            [nameof(MemberSortOption.DebatesAsc)] = "Debates count (fewest debates first)",
            [nameof(MemberSortOption.NameAsc)] = "Name (A-Z)",
            [nameof(MemberSortOption.NameDesc)] = "Name (Z-A)"
        };
    }
}
