# HackerNews.API

A RESTful API built with ASP.NET Core to retrieve the details of the best n stories from the Hacker News API, ordered by score in descending order.

## How to Run

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Running the API

```bash
cd HackerNews.API
dotnet run
```

The API will start:
- HTTP: `http://localhost:5226`
- HTTPS: `https://localhost:7209`

### Usage

```
GET /api/hackernews/best-stories?storiesCount={n}
```

**Example:** Get the top 5 best stories:

```bash
curl https://localhost:7209/api/hackernews/best-stories?storiesCount=5
```

**Response:**

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  },
  { "..." },
  { "..." }
]
```

### Swagger

When running in Development mode, Swagger UI is available at:

```
https://localhost:7209/swagger
```

### Running the Tests

```bash
# Unit tests
dotnet test HackerNews.Tests.Unit

# Integration tests (calls the real Hacker News API)
dotnet test HackerNews.Tests.Integration

# All tests
dotnet test
```

---

## The Proposed Problem

To figure out the right solution for this coding test, we need to first whats exactly the problem we have been facing. So. it seems that we have two parts: get the best stories id calling an external api and then for every id retrieved into the first endpoint, we need to execute the another one to get the details story given the story id.

This a typical network task, which will be processed by the network adapter and we like to say as IO Bound. IO Bound are problems that involves IO like network, disk (hd, ssd), memory ram. They have physical parts into the machine responsible for processing the information and then return for who requested.

In the other hand we have problems regarding CPU Bound, which are problems to be solved/processed by the CPU/CORE. For this sort of problems we can say math operations  or algorithms for example.

So if we have a IO Bound problem (network task to execute) don't make sense allocate the CPU for this task, because if we did it, we waste CPU resources for something which will be executed for another device (network adapter, hd, ram, etc). So we can eliminate  net resources like:

- `Task.Run()` -> Create a thread by using the thread pool (wasting  resourcing)
- `Thread` -> Create a thread into the SO (wasting resourcing)

Finally we found the right resource: `async` / `await` from Task library.

Why: using `async` / `await` we can perform tasks almost the same releasing the threads (thread  pool) with out blocking the main thread. That's perfect for this scenario.

The challenge says that we cannot overload the external API. My strategy is:

1. We fetch the ids.
2. We use `Task.WhenAll` to perform the api requests using concurrency.

If we only using the `Task.WhenAll` without say how many tasks we can run almost the same time (concurrency), we create the overload into the external API. For this problem we can use the `SemaphoreSlim`. `SemaphoreSlim` is a .NET resource to control how many tasks will be executed into that code (concurrently) which is using `SemaphoreSlim`.

3. Execute the details endpoint using `SemaphoreSlim` executing 10 tasks almost the same time.
4. We cached the result to increase the performance endpoint for the next execution.

Also we use the `polly` library and a http client factory to applied some retry rules. When the api fails the polly will execute the endpoint again for x seconds using a exponencial backoff.

**Note 1:** If this would be a real production scenario, I will recommend using background services to process the information and then save into a database or something like that. The endpoint just retrieve the data from the database.

**Note 2:** I keep some "magic numbers" (clean code concept) into the code, but I marked todo into the code saying that these values numbers should be moved to the appsetttings.

## Tech Stack

- **ASP.NET Core 10** - Web framework
- **SimpleSoft.Mediator** - CQRS / Mediator pattern
- **Polly** - Resilience and transient fault handling (retry)
- **HttpClientFactory** - Efficient HTTP client management
- **MemoryCache** - In-memory caching
- **xUnit + Moq + FluentAssertions** - Testing