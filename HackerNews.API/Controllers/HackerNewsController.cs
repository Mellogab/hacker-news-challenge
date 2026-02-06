using HackerNews.Domain.Entities;
using HackerNews.Domain.Queries.Story;
using Microsoft.AspNetCore.Mvc;
using SimpleSoft.Mediator;

namespace HackerNews.API.Controllers;

/// <summary>
/// Controller for hacker news
/// Provides endpoints for return the best stories from the hacker news api
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
    /// <response code="200">Returns the best stories list given the count parameter</response>
    /// <response code="500">If an error occurs while processing the request</response>
    [HttpGet("best-stories")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyCollection<Story>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBestStoriesAsync([FromQuery] int storiesCount, CancellationToken ct)
    {
        try
        {
            var result = await mediator.FetchAsync(new GetBestStoriesQuery(storiesCount), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Hacker News API request");
            return StatusCode(500, new { error = "An error occurred while processing the Hacker News API request" });
        }
    }
}