using FluentAssertions;
using HackerNews.Domain.Queries.Stories;
using HackerNews.Domain.Queries.Story;
using HackerNews.Infrastructure.Interfaces;
using HackerNews.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace HackerNews.Tests.Unit.Handlers;

public class GetBestStoriesQueryHandlerTests
{
    private readonly Mock<IHackerNewsService> _hackerNewsServiceMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<GetBestStoriesQueryHandler>> _loggerMock;
    private readonly GetBestStoriesQueryHandler _handler;

    public GetBestStoriesQueryHandlerTests()
    {
        _hackerNewsServiceMock = new Mock<IHackerNewsService>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<GetBestStoriesQueryHandler>>();
        _handler = new GetBestStoriesQueryHandler(
            _hackerNewsServiceMock.Object,
            _memoryCache,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnStories_WhenServiceReturnsValidData()
    {
        // Arrange
        var storyIds = new[] { 1, 2 };
        var storyModel1 = CreateStoryModel(1, "Story 1", 100);
        var storyModel2 = CreateStoryModel(2, "Story 2", 200);

        _hackerNewsServiceMock
            .Setup(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyIds);

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyModel1);

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyModel2);

        // Act
        var result = await _handler.HandleAsync(new GetBestStoriesQuery(10), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Title == "Story 1" && s.Score == 100);
        result.Should().Contain(s => s.Title == "Story 2" && s.Score == 200);

        _hackerNewsServiceMock.Verify(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _hackerNewsServiceMock.Verify(s => s.GetDetailsByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnStoriesOrderedByScoreDescending()
    {
        // Arrange
        var storyIds = new[] { 1, 2, 3 };

        _hackerNewsServiceMock
            .Setup(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyIds);

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStoryModel(1, "Low Score", 50));

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStoryModel(2, "High Score", 300));

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStoryModel(3, "Mid Score", 150));

        // Act
        var result = await _handler.HandleAsync(new GetBestStoriesQuery(10), CancellationToken.None);

        // Assert
        var scores = result.Select(s => s.Score).ToList();
        scores.Should().BeInDescendingOrder();
        scores.Should().ContainInOrder(300, 150, 50);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnOnlyNStories_WhenCountIsSpecified()
    {
        // Arrange
        var storyIds = new[] { 1, 2, 3 };

        _hackerNewsServiceMock
            .Setup(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyIds);

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStoryModel(1, "Story 1", 300));

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStoryModel(2, "Story 2", 200));

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStoryModel(3, "Story 3", 100));

        // Act
        var result = await _handler.HandleAsync(new GetBestStoriesQuery(2), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Score.Should().Be(300);
        result.Last().Score.Should().Be(200);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCachedStories_WhenCacheHit()
    {
        // Arrange
        var cachedStories = new StoryModel[]
        {
            CreateStoryModel(1, "Cached Story", 500)
        };

        _memoryCache.Set("best_stories", (IReadOnlyCollection<StoryModel>)cachedStories, TimeSpan.FromMinutes(10));

        // Act
        var result = await _handler.HandleAsync(new GetBestStoriesQuery(10), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Cached Story");
        result.First().Score.Should().Be(500);

        _hackerNewsServiceMock.Verify(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()), Times.Never);
        _hackerNewsServiceMock.Verify(s => s.GetDetailsByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoStoriesFound()
    {
        // Arrange
        _hackerNewsServiceMock
            .Setup(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<int>());

        // Act
        var result = await _handler.HandleAsync(new GetBestStoriesQuery(10), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _hackerNewsServiceMock.Verify(s => s.GetDetailsByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenGetBestStoriesIdsFails()
    {
        // Arrange
        _hackerNewsServiceMock
            .Setup(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        // Act
        var act = () => _handler.HandleAsync(new GetBestStoriesQuery(10), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("API unavailable");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenGetDetailsByIdFails()
    {
        // Arrange
        var storyIds = new[] { 1 };

        _hackerNewsServiceMock
            .Setup(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyIds);

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Failed to get story details"));

        // Act
        var act = () => _handler.HandleAsync(new GetBestStoriesQuery(10), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Failed to get story details");
    }

    [Fact]
    public async Task HandleAsync_ShouldConvertUnixTimeToDateTimeOffset()
    {
        // Arrange
        var storyIds = new[] { 1 };
        var unixTime = 1175714200L;
        var expectedTime = DateTimeOffset.FromUnixTimeSeconds(unixTime);

        var storyModel = new StoryModel
        {
            Title = "Test",
            PostedBy = "user",
            Score = 10,
            CommentCount = 5,
            Time = unixTime,
            Uri = new Uri("https://example.com")
        };

        _hackerNewsServiceMock
            .Setup(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyIds);

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyModel);

        // Act
        var result = await _handler.HandleAsync(new GetBestStoriesQuery(10), CancellationToken.None);

        // Assert
        result.First().Time.Should().Be(expectedTime);
    }

    [Fact]
    public async Task HandleAsync_ShouldCacheStoriesAfterFetching()
    {
        // Arrange
        var storyIds = new[] { 1 };
        var storyModel = CreateStoryModel(1, "Story to cache", 300);

        _hackerNewsServiceMock
            .Setup(s => s.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyIds);

        _hackerNewsServiceMock
            .Setup(s => s.GetDetailsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyModel);

        // Act
        await _handler.HandleAsync(new GetBestStoriesQuery(10), CancellationToken.None);

        // Assert
        _memoryCache.TryGetValue("best_stories", out object? cached).Should().BeTrue();
        cached.Should().NotBeNull();
    }

    private static StoryModel CreateStoryModel(int id, string title, int score)
    {
        return new StoryModel
        {
            Title = title,
            PostedBy = $"user{id}",
            Score = score,
            CommentCount = id * 10,
            Time = 1175714200 + id,
            Uri = new Uri($"https://example.com/{id}")
        };
    }
}
