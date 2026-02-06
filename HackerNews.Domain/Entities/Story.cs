
namespace HackerNews.Domain.Entities;

public class Story : IHaveMetaCreated
{
    public string Title { get; set; }
    public Uri Uri { get; set; }
    public string PostedBy { get; set; }
    public DateTimeOffset Time { get; set; }
    public int Score { get; set; }
    public int CommentCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedOn { get; set; }
}
