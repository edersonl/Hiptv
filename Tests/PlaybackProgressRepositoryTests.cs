using Xunit;
using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.ValueObjects;
using IptvStarterApp.Infrastructure.Persistence;

namespace IptvStarterApp.Tests;

public sealed class PlaybackProgressRepositoryTests
{
    [Fact]
    public async Task SaveAsync_ShouldStoreProgress()
    {
        var repository = new PlaybackProgressRepository();

        var progress = new PlaybackProgress(
            new MediaId("movie-1"),
            TimeSpan.FromMinutes(10),
            TimeSpan.FromMinutes(120),
            DateTimeOffset.UtcNow);

        await repository.SaveAsync(progress);

        var loaded = await repository.GetAsync(
            "local",
            new MediaId("movie-1"));

        Assert.NotNull(loaded);
        Assert.Equal(progress.MediaId, loaded!.MediaId);
    }
}