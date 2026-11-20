using LudusLogicae.Api.Core.Common.Constants;
using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Features.Debates.Services;
using LudusLogicae.Api.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LudusLogicae.Api.Features.Debates.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class DebatesController(IDebateService debateService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDebateRequest request)
    {
        var result = await debateService.CreateAsync(request, HttpContext.RequestAborted);

        if (result.StatusCode == StatusCodes.Status201Created && result.CreatedId.HasValue)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.CreatedId.Value }, new { response = result.Envelope });
        }

        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await debateService.GetByIdAsync(id, HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpGet]
    [ProducesResponseType(typeof(LudusLogicaeResponse<List<DebateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] int page = PaginationConstants.DefaultPage,
        [FromQuery] int pageSize = PaginationConstants.DefaultPageSize)
    {
        var result = await debateService.ListAsync(
            status,
            page,
            pageSize,
            HttpContext.RequestAborted);

        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDebateRequest request)
    {
        var result = await debateService.UpdateAsync(id, request, HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await debateService.DeleteAsync(id, HttpContext.RequestAborted);

        if (result.StatusCode == StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpPost("{id:guid}/fully-run")]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateState>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateState>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LudusLogicaeResponse<DebateState>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FullyRun(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await debateService.FullyRunDebateAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result.Envelope);
    }
}