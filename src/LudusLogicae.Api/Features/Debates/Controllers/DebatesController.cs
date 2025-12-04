using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services;
using LudusLogicae.Api.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LudusLogicae.Api.Features.Debates.Controllers;

/// <summary>
///     API endpoints for managing debates in the debate system.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class DebatesController(IDebateService debateService) : ControllerBase
{
    /// <summary>
    ///     Create a new debate.
    /// </summary>
    /// <param name="request">The debate creation request.</param>
    /// <returns>The created debate.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDebateRequest request)
    {
        var (statusCode, envelope, createdId) = await debateService.CreateAsync(request, HttpContext.RequestAborted);

        if (statusCode == StatusCodes.Status201Created && createdId.HasValue)
            return CreatedAtAction(nameof(GetById), new { id = createdId.Value }, new { response = envelope });

        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Get a single debate by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the debate.</param>
    /// <returns>The debate details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var (statusCode, envelope) = await debateService.GetByIdAsync(id, HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     List debates with optional filters and pagination.
    /// </summary>
    /// <param name="status">Filter by debate status (Created, InProgress, Completed).</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of debates.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(LudusLogicaeResponse<List<DebateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] int page = PaginationConstants.DefaultPage,
        [FromQuery] int pageSize = PaginationConstants.DefaultPageSize)
    {
        var (statusCode, envelope) = await debateService.ListAsync(
            status,
            page,
            pageSize,
            HttpContext.RequestAborted);

        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Update an existing debate (only if status is Created).
    /// </summary>
    /// <param name="id">The unique identifier of the debate to update.</param>
    /// <param name="request">The debate update request.</param>
    /// <returns>The updated debate.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDebateRequest request)
    {
        var (statusCode, envelope) = await debateService.UpdateAsync(id, request, HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Delete a debate by ID (only if status is Created).
    /// </summary>
    /// <param name="id">The unique identifier of the debate to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var (statusCode, envelope) = await debateService.DeleteAsync(id, HttpContext.RequestAborted);

        if (statusCode == StatusCodes.Status204NoContent) return NoContent();

        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Fully run a debate and execute the complete debate flow.
    /// </summary>
    /// <param name="id">The unique identifier of the debate to run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The complete debate state after execution.</returns>
    [HttpPost("{id:guid}/fully-run")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateState>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateState>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateState>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FullyRun(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var (statusCode, envelope) = await debateService.FullyRunDebateAsync(id, cancellationToken);
        return StatusCode(statusCode, envelope);
    }
}