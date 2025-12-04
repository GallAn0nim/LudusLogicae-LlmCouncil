using LudusLogicae.Api.Core.Common.Constants;

namespace LudusLogicae.Api.Features.Members.Contracts;

public sealed record MemberListQuery(
    string? Status = null,
    string? Provider = null,
    string? Model = null,
    string? SearchTerm = null,
    int? MinWins = null,
    int? MinPoints = null,
    string? SortBy = null,
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize);
