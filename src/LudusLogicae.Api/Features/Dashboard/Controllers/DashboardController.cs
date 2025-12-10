using LudusLogicae.Api.Features.Dashboard.Contracts;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Services.Data;
using LudusLogicae.Api.Features.Members.Entities;
using LudusLogicae.Api.Features.Members.Services.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Features.Dashboard.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IMemberRepository _memberRepository;
    private readonly IDebateRepository _debateRepository;

    public DashboardController(
        IMemberRepository memberRepository,
        IDebateRepository debateRepository)
    {
        _memberRepository = memberRepository;
        _debateRepository = debateRepository;
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatsResponse>> GetStats(CancellationToken cancellationToken)
    {
        // Execute queries sequentially to avoid "A second operation was started on this context instance" error.
        // EF Core DbContext is not thread-safe.
        var totalMembers = await _memberRepository.BuildListQuery(null, null, null, null, null, null).CountAsync(cancellationToken);
        var activeMembers = await _memberRepository.BuildListQuery("Active", null, null, null, null, null).CountAsync(cancellationToken);
        var benchedMembers = await _memberRepository.BuildListQuery("Benched", null, null, null, null, null).CountAsync(cancellationToken);
        var retiredMembers = await _memberRepository.BuildListQuery("Retired", null, null, null, null, null).CountAsync(cancellationToken);

        var totalDebates = await _debateRepository.BuildListQuery(null).CountAsync(cancellationToken);
        var readyDebates = await _debateRepository.BuildListQuery("Created").CountAsync(cancellationToken);
        var completedDebates = await _debateRepository.BuildListQuery("Completed").CountAsync(cancellationToken);
        var failedDebates = await _debateRepository.BuildListQuery("Failed").CountAsync(cancellationToken);

        var response = new DashboardStatsResponse
        {
            Members = new MemberStats
            {
                Total = totalMembers,
                Active = activeMembers,
                Benched = benchedMembers,
                Retired = retiredMembers
            },
            Debates = new DebateStats
            {
                Total = totalDebates,
                Ready = readyDebates,
                Completed = completedDebates,
                Failed = failedDebates
            }
        };

        return Ok(response);
    }
}
