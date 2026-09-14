using IptvStarterApp.Domain.Playback;
using Xunit;

namespace IptvStarterApp.Tests;

public sealed class PlaybackContractTests
{
    [Fact]
    public void PlaybackRequest_DetectsHlsCaseInsensitively()
    {
        var request = new PlaybackRequest("https://stream.test/channel.M3U8?token=redacted");

        Assert.True(request.IsHls);
    }

    [Fact]
    public void PlaybackRequest_DoesNotDetectOtherFormatsAsHls()
    {
        var request = new PlaybackRequest("https://stream.test/movie.mp4");

        Assert.False(request.IsHls);
    }
}
