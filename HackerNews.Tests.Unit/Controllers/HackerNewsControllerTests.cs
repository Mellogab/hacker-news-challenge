using FluentAssertions;
using HackerNews.API.Controllers;
using HackerNews.Domain.Entities;
using HackerNews.Domain.Queries.Story;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleSoft.Mediator;

namespace HackerNews.Tests.Unit.Controllers;

public class HackerNewsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<HackerNewsController>> _loggerMock;
    private readonly HackerNewsController _controller;

    public HackerNewsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<HackerNewsController>>();
        _controller = new HackerNewsController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldReturn200_WhenSuccess()
    {
        // Arrange
        var stories = new List<Story>
        {
            new()
            {
                Title = "Test Story",
                PostedBy = "user1",
                Score = 100,
                CommentCount = 50,
                Time = DateTimeOffset.UtcNow,
                Uri = new Uri("https://example.com"),
                CreatedAt = DateTimeOffset.Now,
                CreatedOn = Environment.MachineName
            }
        }.AsReadOnly();

        _mediatorMock
            .Setup(m => m.FetchAsync(It.IsAny<GetBestStoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stories);

        // Act
        var result = await _controller.GetBestStoriesAsync(10, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedStories = okResult.Value.Should().BeAssignableTo<IReadOnlyCollection<Story>>().Subject;
        returnedStories.Should().HaveCount(1);
        returnedStories.First().Title.Should().Be("Test Story");
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.FetchAsync(It.IsAny<GetBestStoriesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Something went wrong"));

        // Act
        var result = await _controller.GetBestStoriesAsync(10, CancellationToken.None);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldReturnEmptyList_WhenNoStories()
    {
        // Arrange
        var emptyStories = new List<Story>().AsReadOnly();

        _mediatorMock
            .Setup(m => m.FetchAsync(It.IsAny<GetBestStoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyStories);

        // Act
        var result = await _controller.GetBestStoriesAsync(10, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedStories = okResult.Value.Should().BeAssignableTo<IReadOnlyCollection<Story>>().Subject;
        returnedStories.Should().BeEmpty();
    }
}
