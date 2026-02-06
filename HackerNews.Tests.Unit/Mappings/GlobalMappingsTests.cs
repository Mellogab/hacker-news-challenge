using FluentAssertions;
using HackerNews.Domain;
using HackerNews.Infrastructure.Models;

namespace HackerNews.Tests.Unit.Mappings;

public class GlobalMappingsTests
{
    [Fact]
    public void ToModel_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var storyModel = new StoryModel
        {
            Title = "Test Story",
            PostedBy = "testuser",
            Score = 150,
            CommentCount = 42,
            Time = 1175714200,
            Uri = new Uri("https://example.com/story")
        };

        // Act
        var result = storyModel.ToModel();

        // Assert
        result.Title.Should().Be("Test Story");
        result.PostedBy.Should().Be("testuser");
        result.Score.Should().Be(150);
        result.CommentCount.Should().Be(42);
        result.Uri.Should().Be(new Uri("https://example.com/story"));
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(5));
        result.CreatedOn.Should().Be(Environment.MachineName);
    }

    [Fact]
    public void ToModel_ShouldConvertUnixTimeToDateTimeOffset()
    {
        // Arrange
        var unixTimestamp = 1570887781L; // 2019-10-12T13:43:01+00:00
        var expectedTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

        var storyModel = new StoryModel
        {
            Title = "Test",
            PostedBy = "user",
            Score = 1,
            CommentCount = 0,
            Time = unixTimestamp,
            Uri = new Uri("https://example.com")
        };

        // Act
        var result = storyModel.ToModel();

        // Assert
        result.Time.Should().Be(expectedTime);
        result.Time.Year.Should().Be(2019);
        result.Time.Month.Should().Be(10);
        result.Time.Day.Should().Be(12);
    }

    [Fact]
    public void ToModel_ShouldHandleZeroUnixTime()
    {
        // Arrange
        var storyModel = new StoryModel
        {
            Title = "Test",
            PostedBy = "user",
            Score = 1,
            CommentCount = 0,
            Time = 0,
            Uri = new Uri("https://example.com")
        };

        // Act
        var result = storyModel.ToModel();

        // Assert
        result.Time.Should().Be(DateTimeOffset.UnixEpoch);
    }
}
