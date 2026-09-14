using Xunit;
using IptvStarterApp.Infrastructure.Playlist;

namespace IptvStarterApp.Tests;

public sealed class M3uParserTests
{
    private readonly M3uParser _parser = new();

    [Fact]
    public void Parse_ValidPlaylist_ReadsKnownAndUnknownAttributes()
    {
        const string content = "#EXTM3U\n#EXTINF:-1 tvg-id=\"br-1\" tvg-name=\"Canal 1\" tvg-logo=\"https://img/logo.png\" group-title=\"News\" custom=\"value\",Canal exibido\nhttps://stream.test/live/1.m3u8";

        var channels = _parser.Parse(content);

        var channel = Assert.Single(channels);
        Assert.Equal("br-1", channel.TvgId);
        Assert.Equal("Canal 1", channel.TvgName);
        Assert.Equal("https://img/logo.png", channel.LogoUrl);
        Assert.Equal("News", channel.Group);
        Assert.Equal("value", channel.Attributes["custom"]);
        Assert.Equal("https://stream.test/live/1.m3u8", channel.Source.AbsoluteUri);
    }

    [Fact]
    public void Parse_InvalidLines_IgnoresThemAndContinues()
    {
        const string content = "#EXTM3U\nlinha sem metadata\n#EXTINF:-1,Valido\nnot-a-url\n#EXTINF:-1,Outro\nhttps://stream.test/2.ts";

        var channels = _parser.Parse(content);

        var channel = Assert.Single(channels);
        Assert.Equal("Outro", channel.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyPlaylist_ReturnsEmpty(string content)
    {
        Assert.Empty(_parser.Parse(content));
    }

    [Fact]
    public void Parse_DuplicateUrls_ReturnsOnlyFirstChannel()
    {
        const string content = "#EXTM3U\n#EXTINF:-1,Primeiro\nhttps://stream.test/same.m3u8\n#EXTINF:-1,Segundo\nhttps://stream.test/same.m3u8";

        var channels = _parser.Parse(content);

        var channel = Assert.Single(channels);
        Assert.Equal("Primeiro", channel.Name);
    }

    [Fact]
    public void Parse_MissingAttributes_UsesSafeDefaults()
    {
        const string content = "#EXTM3U\n#EXTINF:-1,Canal sem atributos\nhttps://stream.test/default.ts";

        var channel = Assert.Single(_parser.Parse(content));

        Assert.Equal("General", channel.Group);
        Assert.Equal("Canal sem atributos", channel.Name);
        Assert.Null(channel.TvgId);
    }

    [Fact]
    public void Parse_InvalidUrls_DoesNotCreateChannels()
    {
        const string content = "#EXTM3U\n#EXTINF:-1,FTP\nftp://stream.test/file.ts\n#EXTINF:-1,Relativa\n/file.ts";

        Assert.Empty(_parser.Parse(content));
    }
}
