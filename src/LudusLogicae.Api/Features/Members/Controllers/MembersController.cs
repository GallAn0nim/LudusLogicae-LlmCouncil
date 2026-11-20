using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Features.Members.Services;
using LudusLogicae.Api.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LudusLogicae.Api.Features.Members.Controllers;

[ApiController]
[Route("api/members")]
[Produces("application/json")]
public sealed class MembersController(IMembersService membersService) : ControllerBase
{
    [HttpPost("{id:guid}/introductions")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberIntroductionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberIntroductionResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberIntroductionResponse>), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Introduce(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await membersService.IntroduceAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpGet]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MembersListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] MemberListQuery query)
    {
        var result = await membersService.ListAsync(query, HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await membersService.GetByIdAsync(id, HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpPost]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        var result = await membersService.CreateAsync(request, HttpContext.RequestAborted);

        if (result.StatusCode == StatusCodes.Status201Created && result.CreatedId.HasValue)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.CreatedId.Value }, new { response = result.Envelope });
        }

        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMemberRequest request)
    {
        var result = await membersService.UpdateAsync(id, request, HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await membersService.DeleteAsync(id, HttpContext.RequestAborted);

        if (result.StatusCode == StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }
}