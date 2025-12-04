namespace LudusLogicae.Api.Features.Members.Contracts;

public class MemberIntroductionResponse
{
    public Guid MemberId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Introduction { get; set; } = string.Empty;
}
