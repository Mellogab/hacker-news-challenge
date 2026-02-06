using HackerNews.Domain.Queries.Story;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using SimpleSoft.Mediator;
using StoryEntity = HackerNews.Domain.Entities.Story;
using HackerNews.Infrastructure.Interfaces;
using HackerNews.Infrastructure.Models;

namespace HackerNews.Domain.Queries.Stories;

public class GetBestStoriesQueryHandler(
    IHackerNewsService hackerNewsService,
    IMemoryCache cache,
    ILogger<GetBestStoriesQueryHandler> logger) : IQueryHandler<GetBestStoriesQuery, IReadOnlyCollection<StoryEntity>>
{
    //TO DO: This values (10, 10, timespan) should be moved to appsettings.json
    private static readonly SemaphoreSlim _semaphore = new(10, 10);
    private const string CacheKey = "best_stories";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<IReadOnlyCollection<StoryEntity>> HandleAsync(GetBestStoriesQuery query, CancellationToken ct)
    {

        if (cache.TryGetValue(CacheKey, out IReadOnlyCollection<StoryModel>? cachedStories) && cachedStories != null)
        {
            logger.LogInformation("Cache hit. Returning {Count} stories.",cachedStories.Count);
            return cachedStories
                .Select(story => story.ToModel())
                .Where(story => story != null)
                .OrderByDescending(story => story.Score)
                .Take(query.Count)
                .ToList()
                .AsReadOnly();
        }

        IEnumerable<int> storiesIds;

        logger.LogInformation("Searching story ids");
        storiesIds = await hackerNewsService.GetBestStoriesIdsAsync(ct);

        if (!storiesIds.Any())
        {
            logger.LogWarning("No stories found.");
            return Array.Empty<StoryEntity>();
        }

        logger.LogInformation("Creating tasks to get story details given story ids returned before.");
        var tasks = storiesIds.Select(async id =>
        {
            await _semaphore.WaitAsync(ct);

            try
            {
                return await hackerNewsService.GetDetailsByIdAsync(id, ct);
            }
            finally
            {
                _semaphore.Release();
            }
        });

        logger.LogInformation("Executing tasks.");
        var stories = await Task.WhenAll(tasks);

        logger.LogInformation("Setting stories into cache.");
        cache.Set(CacheKey, stories, CacheExpiration);

        logger.LogInformation("Filtering stories and return.");
        return stories
            .Select(story => story.ToModel())
            .Where(story => story != null)
            .OrderByDescending(story => story.Score)
            .Take(query.Count)
            .ToList()
            .AsReadOnly();
    }
}
