using LudusLogicae.Api.Features.Metadata.Services;
using Microsoft.AspNetCore.Mvc;

namespace LudusLogicae.Api.Features.Metadata.Controllers;

/// <summary>
///     API endpoints for system metadata and dictionaries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class MetadataController(IMetadataService metadataService) : ControllerBase
{
    /// <summary>
    ///     Lightweight liveness probe.
    /// </summary>
    /// <returns>Pong response.</returns>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Ping()
    {
        var (statusCode, envelope) = await metadataService.PingAsync(HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Returns concise metadata about the Ludus Logicae project and runtime.
    /// </summary>
    /// <returns>System information.</returns>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Info()
    {
        var (statusCode, envelope) = await metadataService.InfoAsync(HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Returns supported LLM providers from configuration.
    /// </summary>
    /// <returns>List of provider names.</returns>
    [HttpGet("dictionary/provider")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviders()
    {
        var (statusCode, envelope) = await metadataService.GetProvidersAsync(HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Returns supported models for a provider identified by name.
    /// </summary>
    /// <param name="id">The provider name (case-insensitive).</param>
    /// <returns>List of supported model names.</returns>
    [HttpGet("dictionary/provider/{id}/models")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProviderModels([FromRoute] string id)
    {
        var (statusCode, envelope) = await metadataService.GetProviderModelsAsync(id, HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }


    /// <summary>
    ///     Returns available debate statuses.
    /// </summary>
    /// <returns>List of debate status values.</returns>
    [HttpGet("dictionary/debate-phase")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDebatePhasees()
    {
        var (statusCode, envelope) = await metadataService.GetDebatePhaseesAsync(HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }

    /// <summary>
    ///     Returns available member sort options with descriptions.
    /// </summary>
    /// <returns>Dictionary of sort option names and their descriptions.</returns>
    [HttpGet("dictionary/member-sort-options")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMemberSortOptions()
    {
        var (statusCode, envelope) = await metadataService.GetMemberSortOptionsAsync(HttpContext.RequestAborted);
        return StatusCode(statusCode, new { response = envelope });
    }
}