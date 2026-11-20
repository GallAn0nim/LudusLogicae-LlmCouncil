using LudusLogicae.Api.Features.Metadata.Services;
using Microsoft.AspNetCore.Mvc;

namespace LudusLogicae.Api.Features.Metadata.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class MetadataController(IMetadataService metadataService) : ControllerBase
{
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Ping()
    {
        var result = await metadataService.PingAsync(HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Info()
    {
        var result = await metadataService.InfoAsync(HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpGet("dictionary/provider")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviders()
    {
        var result = await metadataService.GetProvidersAsync(HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpGet("dictionary/provider/{id}/models")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProviderModels([FromRoute] string id)
    {
        var result = await metadataService.GetProviderModelsAsync(id, HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpGet("dictionary/debate-phase")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDebatePhasees()
    {
        var result = await metadataService.GetDebatePhaseesAsync(HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }

    [HttpGet("dictionary/member-sort-options")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMemberSortOptions()
    {
        var result = await metadataService.GetMemberSortOptionsAsync(HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, new { response = result.Envelope });
    }
}