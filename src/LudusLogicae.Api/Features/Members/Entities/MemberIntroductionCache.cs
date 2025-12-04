namespace LudusLogicae.Api.Features.Members.Entities;

public class MemberIntroductionCache
{
    public int Id { get; set; }
    public Guid MemberId { get; set; }
    public string Introduction { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Member Member { get; set; } = null!;
}
