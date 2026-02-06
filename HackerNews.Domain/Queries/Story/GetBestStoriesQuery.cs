using SimpleSoft.Mediator;
using StoryEntity = HackerNews.Domain.Entities.Story;

namespace HackerNews.Domain.Queries.Story;

public class GetBestStoriesQuery(int count) : Query<IReadOnlyCollection<StoryEntity>>
{
    public int Count { get; } = count;
}
