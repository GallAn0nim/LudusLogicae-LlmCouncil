using LudusLogicae.Api.Features.Members.Contracts;
using LudusLogicae.Api.Features.Members.Services;
using LudusLogicae.Api.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LudusLogicae.Api.Features.Members.Controllers;

/// <summary>
///     API endpoints for managing AI members in the debate system.
/// </summary>
[ApiController]
[Route("api/members")]
[Produces("application/json")]
public sealed class MembersController(IMembersService membersService) : ControllerBase
{
    /// <summary>
    ///     Generate an introduction for this member via its configured LLM provider.
    /// </summary>
    /// <param name="id">The unique identifier of the member.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated introduction.</returns>
    [HttpPost("{id:guid}/introductions")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberIntroductionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberIntroductionResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberIntroductionResponse>), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Introduce(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var (statusCode, envelope) = await membersService.IntroduceAsync(id, cancellationToken);
        return StatusCode(statusCode, new { response = envelope });
    }


    /// <summary>
    ///     List members with optional filters and pagination.
    /// </summary>
    /// <param name="query">Query parameters for filtering, searching, and pagination.</param>
    /// <returns>A paginated list of members.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MembersListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] MemberListQuery query)
    {
        var (statusCode, envelope) = await membersService.ListAsync(query, HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Get a single member by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the member.</param>
    /// <returns>The member details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var (statusCode, envelope) = await membersService.GetByIdAsync(id, HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Create a new member.
    /// </summary>
    /// <param name="request">The member creation request.</param>
    /// <returns>The created member.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        var (statusCode, envelope, createdId) = await membersService.CreateAsync(request, HttpContext.RequestAborted);

        if (statusCode == StatusCodes.Status201Created && createdId.HasValue)
            return CreatedAtAction(nameof(GetById), new { id = createdId.Value }, new { response = envelope });

        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Update an existing member (full update).
    /// </summary>
    /// <param name="id">The unique identifier of the member to update.</param>
    /// <param name="request">The member update request.</param>
    /// <returns>The updated member.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMemberRequest request)
    {
        var (statusCode, envelope) = await membersService.UpdateAsync(id, request, HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Delete a member by ID (hard delete).
    /// </summary>
    /// <param name="id">The unique identifier of the member to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<MemberDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var (statusCode, envelope) = await membersService.DeleteAsync(id, HttpContext.RequestAborted);

        if (statusCode == StatusCodes.Status204NoContent) return NoContent();

        return StatusCode(statusCode, new { response = envelope! });
    }
}