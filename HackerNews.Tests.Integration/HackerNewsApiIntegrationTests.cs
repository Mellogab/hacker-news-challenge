using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HackerNews.Tests.Integration;

public class HackerNewsApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HackerNewsApiIntegrationTests(WebApplicationFactory<Program> factory) =>
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    [Fact]
    public async Task GetBestStories_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/hackernews/border-flow?n=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetBestStories_ResponseShouldBeValidJsonArray()
    {
        // Act
        var response = await _client.GetAsync("/api/hackernews/border-flow?n=5");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stories = JsonSerializer.Deserialize<JsonElement>(content);
        stories.ValueKind.Should().Be(JsonValueKind.Array);
        stories.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetBestStories_EachStoryShouldHaveExpectedFields()
    {
        // Act
        var response = await _client.GetAsync("/api/hackernews/border-flow?n=5");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var stories = JsonSerializer.Deserialize<JsonElement>(content);
        var firstStory = stories.EnumerateArray().First();

        firstStory.TryGetProperty("title", out _).Should().BeTrue();
        firstStory.TryGetProperty("postedBy", out _).Should().BeTrue();
        firstStory.TryGetProperty("time", out _).Should().BeTrue();
        firstStory.TryGetProperty("score", out _).Should().BeTrue();
        firstStory.TryGetProperty("commentCount", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetBestStories_ScoreShouldBePositiveInteger()
    {
        // Act
        var response = await _client.GetAsync("/api/hackernews/border-flow?n=5");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var stories = JsonSerializer.Deserialize<JsonElement>(content);

        foreach (var story in stories.EnumerateArray().Take(5))
        {
            story.GetProperty("score").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetBestStories_TimeShouldBeValidDateTimeString()
    {
        // Act
        var response = await _client.GetAsync("/api/hackernews/border-flow?n=5");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var stories = JsonSerializer.Deserialize<JsonElement>(content);
        var firstStory = stories.EnumerateArray().First();

        var timeValue = firstStory.GetProperty("time").GetString();
        timeValue.Should().NotBeNullOrEmpty();

        var parsed = DateTimeOffset.TryParse(timeValue, out var parsedTime);
        parsed.Should().BeTrue();
        parsedTime.Year.Should().BeGreaterThan(2000);
    }

    [Fact]
    public async Task GetBestStories_InvalidEndpoint_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/hackernews/nonexistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
