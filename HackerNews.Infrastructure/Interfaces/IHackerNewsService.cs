using HackerNews.Infrastructure.Models;

namespace HackerNews.Infrastructure.Interfaces;

public interface IHackerNewsService
{
    public Task<IEnumerable<int>> GetBestStoriesIdsAsync(CancellationToken cancellationToken);
    public Task<StoryModel> GetDetailsByIdAsync(int id, CancellationToken cancellationToken);
}
