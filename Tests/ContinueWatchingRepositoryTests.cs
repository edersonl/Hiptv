using Xunit;
using IptvStarterApp.Infrastructure.Persistence;

namespace IptvStarterApp.Tests;

public sealed class ContinueWatchingRepositoryTests
{
    [Fact]
    public void Repository_CanBeCreated()
    {
        var progressRepository =
            new PlaybackProgressRepository();

        var repository =
            new ContinueWatchingRepository(progressRepository);

        Assert.NotNull(repository);
    }
}