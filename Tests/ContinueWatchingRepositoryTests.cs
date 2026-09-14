using Xunit;
using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.ValueObjects;
using IptvStarterApp.Infrastructure.Persistence;

namespace IptvStarterApp.Tests;

public sealed class ContinueWatchingRepositoryTests
{
    [Fact]
    public async Task InProgressItem_ShouldBeReturned()
    {
        var progressRepository =
            new PlaybackProgressRepository();

        var contentRepository =
            new FakeContentRepository();

        var movie = new Movie(
            new MediaId("movie-1"),
            "Matrix",
            new Uri("https://example.com/movie.mp4"));

        await contentRepository.SaveAsync(movie);

        await progressRepository.SaveAsync(
            new PlaybackProgress(
                movie.Id,
                TimeSpan.FromMinutes(10),
                TimeSpan.FromMinutes(120),
                DateTimeOffset.UtcNow));

        var repository =
            new ContinueWatchingRepository(
                progressRepository,
                contentRepository);

        var result =
            await repository.ListAsync("local");

        Assert.Single(result);
    }

    [Fact]
    public async Task CompletedItem_ShouldNotBeReturned()
    {
        var progressRepository =
            new PlaybackProgressRepository();

        var contentRepository =
            new FakeContentRepository();

        var movie = new Movie(
            new MediaId("movie-2"),
            "Interestelar",
            new Uri("https://example.com/movie2.mp4"));

        await contentRepository.SaveAsync(movie);

        await progressRepository.SaveAsync(
            new PlaybackProgress(
                movie.Id,
                TimeSpan.FromMinutes(118),
                TimeSpan.FromMinutes(120),
                DateTimeOffset.UtcNow));

        var repository =
            new ContinueWatchingRepository(
                progressRepository,
                contentRepository);

        var result =
            await repository.ListAsync("local");

        Assert.Empty(result);
    }

    [Fact]
    public async Task MoreRecentItems_ShouldAppearFirst()
    {
        var progressRepository =
            new PlaybackProgressRepository();

        var contentRepository =
            new FakeContentRepository();

        var movie1 = new Movie(
            new MediaId("movie-1"),
            "Matrix",
            new Uri("https://example.com/movie1.mp4"));

        var movie2 = new Movie(
            new MediaId("movie-2"),
            "Interestelar",
            new Uri("https://example.com/movie2.mp4"));

        await contentRepository.SaveAsync(movie1);
        await contentRepository.SaveAsync(movie2);

        await progressRepository.SaveAsync(
            new PlaybackProgress(
                movie1.Id,
                TimeSpan.FromMinutes(20),
                TimeSpan.FromMinutes(120),
                DateTimeOffset.UtcNow.AddMinutes(-10)));

        await progressRepository.SaveAsync(
            new PlaybackProgress(
                movie2.Id,
                TimeSpan.FromMinutes(30),
                TimeSpan.FromMinutes(120),
                DateTimeOffset.UtcNow));

        var repository =
            new ContinueWatchingRepository(
                progressRepository,
                contentRepository);

        var result =
            await repository.ListAsync("local");

        Assert.Equal("Interestelar", result[0].MediaItem.Name);
        Assert.Equal("Matrix", result[1].MediaItem.Name);
    }

 [Fact]
public async Task ListAsync_ShouldRespectLimit()
{
    var progressRepository =
        new PlaybackProgressRepository();

    var contentRepository =
        new FakeContentRepository();

    var movie1 = new Movie(
        new MediaId("movie-1"),
        "Matrix",
        new Uri("https://example.com/movie1.mp4"));

    var movie2 = new Movie(
        new MediaId("movie-2"),
        "Interestelar",
        new Uri("https://example.com/movie2.mp4"));

    await contentRepository.SaveAsync(movie1);
    await contentRepository.SaveAsync(movie2);

    await progressRepository.SaveAsync(
        new PlaybackProgress(
            movie1.Id,
            TimeSpan.FromMinutes(20),
            TimeSpan.FromMinutes(120),
            DateTimeOffset.UtcNow.AddMinutes(-10)));

    await progressRepository.SaveAsync(
        new PlaybackProgress(
            movie2.Id,
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(120),
            DateTimeOffset.UtcNow));

    var repository =
        new ContinueWatchingRepository(
            progressRepository,
            contentRepository);

    var result =
        await repository.ListAsync(
            "local",
            1);

    Assert.Single(result);

    Assert.Equal(
        "Interestelar",
        result[0].MediaItem.Name);
}
}