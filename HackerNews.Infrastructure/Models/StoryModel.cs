using System.Text.Json.Serialization;

namespace HackerNews.Infrastructure.Models;

public class StoryModel
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("url")]
    public Uri Uri { get; set; }

    [JsonPropertyName("by")]
    public string PostedBy { get; set; }

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("descendants")]
    public int CommentCount { get; set; }
}
