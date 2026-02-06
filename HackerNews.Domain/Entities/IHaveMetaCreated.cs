namespace HackerNews.Domain.Entities;

public interface IHaveMetaCreated
{
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedOn { get; set; }
}
