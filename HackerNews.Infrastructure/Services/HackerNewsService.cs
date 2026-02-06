using HackerNews.Infrastructure.Models;
using HackerNews.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HackerNews.Infrastructure.Services;

public class HackerNewsService(HttpClient httpClient, ILogger<HackerNewsService> logger) : IHackerNewsService
{
    public async Task<IEnumerable<int>> GetBestStoriesIdsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var ids = await httpClient.GetFromJsonAsync<int[]>(
                "v0/beststories.json", //TO DO: This url should be into appsettings.json
                cancellationToken);

            return ids ?? Array.Empty<int>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "One error has ocurred when trying to get the best stories from hacker news api");
            throw new Exception("One error has ocurred when trying to get the best stories from hacker news api", ex);
        }
    }

    public async Task<StoryModel> GetDetailsByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var story = await httpClient.GetFromJsonAsync<StoryModel>(
                $"v0/item/{id}.json", //TO DO: This url should be into appsettings.json
                cancellationToken
            );

            return story;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "One error has ocurred to get the story details: {id}", id);
            throw new Exception("One error has ocurred to get the story details: {id}", ex);
        }
    }
}
