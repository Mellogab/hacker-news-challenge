using HackerNews.Domain.Entities;
using HackerNews.Infrastructure.Models;

namespace HackerNews.Domain;

public static class GlobalMappings
{
    public static Story ToModel(this StoryModel story)
        => new Story {
            CommentCount = story.CommentCount,
            PostedBy = story.PostedBy,
            Score = story.Score,
            Time = DateTimeOffset.FromUnixTimeSeconds(story.Time),
            Title = story.Title,
            Uri = story.Uri,
            CreatedAt = DateTimeOffset.Now,
            CreatedOn = Environment.MachineName
        };
}
