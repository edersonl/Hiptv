using Xunit;
using IptvStarterApp.Infrastructure.Persistence;

namespace IptvStarterApp.Tests;

public sealed class ContinueWatchingRepositoryTests
{
    [Fact]
    public void Repository_CanBeCreated()
    {
        var playbackRepository =
            new PlaybackProgressRepository();

        var repository =
            new ContinueWatchingRepository(playbackRepository);

        Assert.NotNull(repository);
    }
}