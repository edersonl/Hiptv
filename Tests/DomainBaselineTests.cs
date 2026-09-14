using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.Enums;
using IptvStarterApp.Domain.ValueObjects;
using Xunit;

namespace IptvStarterApp.Tests;

public sealed class DomainBaselineTests
{
    private static readonly Uri StreamUri = new("https://stream.test/content.m3u8");

    [Fact]
    public void ValueObjects_WithEquivalentNormalizedValues_AreEqual()
    {
        Assert.Equal(new MediaId("media-1"), new MediaId(" media-1 "));
        Assert.Equal(new PlaylistId("playlist-1"), new PlaylistId(" playlist-1 "));
        Assert.Equal(new SeasonId("season-1"), new SeasonId(" season-1 "));
        Assert.Equal(new CategoryId("category-1"), new CategoryId(" category-1 "));
    }

    [Fact]
    public void ValueObjects_RejectEmptyIdentifiers()
    {
        Assert.Throws<ArgumentException>(() => new MediaId(" "));
        Assert.Throws<ArgumentException>(() => new PlaylistId(" "));
        Assert.Throws<ArgumentException>(() => new SeasonId(" "));
        Assert.Throws<ArgumentException>(() => new CategoryId(" "));
    }

    [Fact]
    public void PlayableEntities_RequireValidNamesAndUrls()
    {
        Assert.Throws<ArgumentException>(() => new Channel(new MediaId("channel"), " ", StreamUri));
        Assert.Throws<ArgumentException>(() => new Movie(new MediaId("movie"), "Filme", new Uri("file:///movie.mp4")));

        var movie = new Movie(new MediaId("movie"), "Filme", StreamUri);

        Assert.Equal(MediaType.Movie, movie.MediaType);
        Assert.Equal(ContentType.VideoOnDemand, movie.ContentType);
    }

    [Fact]
    public void Season_RejectsEpisodeFromAnotherSeason()
    {
        var seriesId = new MediaId("series-1");
        var seasonId = new SeasonId("season-1");
        var episode = new Episode(
            new MediaId("episode-1"), "Episódio 1", StreamUri,
            seriesId, new SeasonId("season-2"), 1, 1);

        Assert.Throws<ArgumentException>(() => new Season(seasonId, seriesId, 1, new[] { episode }));
    }

    [Fact]
    public void Series_RejectsSeasonFromAnotherSeries()
    {
        var season = new Season(new SeasonId("season-1"), new MediaId("series-2"), 1);

        Assert.Throws<ArgumentException>(() =>
            new Series(new MediaId("series-1"), "Série", new[] { season }));
    }

    [Fact]
    public void Series_OrdersSeasonsAndEpisodesAndPreservesAggregateIntegrity()
    {
        var seriesId = new MediaId("series-1");
        var seasonOneId = new SeasonId("season-1");
        var episodeTwo = new Episode(new MediaId("episode-2"), "Episódio 2", StreamUri, seriesId, seasonOneId, 1, 2);
        var episodeOne = new Episode(new MediaId("episode-1"), "Episódio 1", StreamUri, seriesId, seasonOneId, 1, 1);
        var seasonTwo = new Season(new SeasonId("season-2"), seriesId, 2);
        var seasonOne = new Season(seasonOneId, seriesId, 1, new[] { episodeTwo, episodeOne });

        var series = new Series(seriesId, "Série", new[] { seasonTwo, seasonOne });

        Assert.Equal(1, series.Seasons[0].Number);
        Assert.Equal(1, series.Seasons[0].Episodes[0].EpisodeNumber);
        Assert.Equal(2, series.TotalSeasons);
        Assert.Equal(2, series.Episodes.Count);
    }

    [Fact]
    public void Favorite_AlwaysReferencesAMediaItem()
    {
        var channel = new Channel(new MediaId("channel-1"), "Canal", StreamUri);
        var favorite = new Favorite(channel, DateTimeOffset.Parse("2026-09-08T12:00:00Z"));

        Assert.Same(channel, favorite.MediaItem);
        Assert.Equal(channel.Id, favorite.MediaId);
        Assert.Throws<ArgumentNullException>(() => new Favorite(null!, DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(29, 600, PlaybackStatus.InProgress, false)]
    [InlineData(30, 600, PlaybackStatus.InProgress, true)]
    [InlineData(540, 600, PlaybackStatus.Completed, false)]
    [InlineData(500, 600, PlaybackStatus.Completed, false)]
    public void PlaybackProgress_AppliesContinueWatchingAndCompletionRules(
        int positionSeconds,
        int durationSeconds,
        PlaybackStatus expectedStatus,
        bool expectedEligibility)
    {
        var progress = new PlaybackProgress(
            new MediaId("movie-1"),
            TimeSpan.FromSeconds(positionSeconds),
            TimeSpan.FromSeconds(durationSeconds),
            DateTimeOffset.Parse("2026-09-08T12:00:00Z"));

        Assert.Equal(expectedStatus, progress.Status);
        Assert.Equal(expectedEligibility, progress.IsEligibleForContinueWatching);
    }

    [Fact]
    public void ContinueWatching_RejectsLiveAndMismatchedProgress()
    {
        var progress = new PlaybackProgress(
            new MediaId("movie-1"),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(10),
            DateTimeOffset.UtcNow);
        var channel = new Channel(new MediaId("movie-1"), "Canal", StreamUri);
        var anotherMovie = new Movie(new MediaId("movie-2"), "Filme", StreamUri);

        Assert.Throws<ArgumentException>(() => new ContinueWatching(channel, progress));
        Assert.Throws<ArgumentException>(() => new ContinueWatching(anotherMovie, progress));
    }

    [Fact]
    public void PlaylistAndCategory_EnforceBusinessRules()
    {
        Assert.Throws<ArgumentException>(() => new Playlist(
            new PlaylistId("playlist"), "Lista", PlaylistType.M3U, new Uri("file:///playlist.m3u")));
        Assert.Throws<ArgumentException>(() => new Category(
            new CategoryId("same"), "Categoria", ContentType.Live, new CategoryId("same")));

        var playlist = new Playlist(
            new PlaylistId("playlist"), "Lista", PlaylistType.M3U,
            new Uri("https://playlist.test/list.m3u"),
            new[] { new MediaId("one"), new MediaId("one") });

        Assert.Single(playlist.ItemIds);
    }
}