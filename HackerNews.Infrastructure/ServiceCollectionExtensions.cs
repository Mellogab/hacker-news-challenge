using HackerNews.Infrastructure.Interfaces;
using HackerNews.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Polly;

namespace Microsoft.Extensions.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHackerNewsInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IHackerNewsService, HackerNewsService>(client =>
        {
            //TO DO: This url should be moved to appsettings.json
            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler(options =>
        {
            //TO DO: This three config values should be moved to appsettings.json
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromMilliseconds(500);
            options.Retry.BackoffType = DelayBackoffType.Exponential;

            //TO DO: This config values should be moved to appsettings.json
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}