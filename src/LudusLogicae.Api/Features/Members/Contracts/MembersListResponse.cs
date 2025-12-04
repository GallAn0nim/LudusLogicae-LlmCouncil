namespace LudusLogicae.Api.Features.Members.Contracts;

public class MembersListResponse
{
    public List<MemberDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
