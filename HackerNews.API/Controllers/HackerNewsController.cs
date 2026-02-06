using HackerNews.Domain.Entities;
using HackerNews.Domain.Queries.Story;
using Microsoft.AspNetCore.Mvc;
using SimpleSoft.Mediator;

namespace HackerNews.API.Controllers;

/// <summary>
/// Controller for hacker news
/// Provides endpoints for border activity analysis including entries, exits, and irregular crossings
/// </summary>
[ApiController]
[Route("api/hackernews")]
[Produces("application/json")]
public class HackerNewsController(IMediator mediator, ILogger<HackerNewsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets the best stories details from hacker news api
    /// </summary>
    /// <returns>List from bes stories</returns>
    /// <response code="200">Returns the border flow overview data</response>
    /// <response code="500">If an error occurs while processing the request</response>
    [HttpGet("border-flow")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyCollection<Story>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBestStoriesAsync([FromQuery] int n, CancellationToken ct)
    {
        try
        {
            var result = await mediator.FetchAsync(new GetBestStoriesQuery(n), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Border Flow Overview request");
            return StatusCode(500, new { error = "An error occurred while processing the border flow overview request" });
        }
    }
}
