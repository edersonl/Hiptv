using Xunit;
using IptvStarterApp.Domain.Interfaces;
using IptvStarterApp.Domain.Playback;
using IptvStarterApp.Infrastructure.Playback;

namespace IptvStarterApp.Tests;

public sealed class PlaybackHardeningTests
{
    [Fact]
    public void Metrics_RecordsStartupBufferingRatioRecoveryAndActivePlayback()
    {
        var now = DateTimeOffset.Parse("2026-09-08T12:00:00Z");
        var metrics = new PlaybackMetrics(() => now);

        metrics.RecordAttempt();
        metrics.RecordState(Domain.Playback.PlaybackState.Buffering);
        now = now.AddSeconds(2);
        metrics.RecordState(Domain.Playback.PlaybackState.Playing);
        now = now.AddSeconds(8);
        metrics.RecordState(Domain.Playback.PlaybackState.Paused);
        metrics.RecordReconnection();

        var snapshot = metrics.Snapshot();

        Assert.Equal(TimeSpan.FromSeconds(2), snapshot.StartupTime);
        Assert.Equal(TimeSpan.FromSeconds(2), snapshot.TimeToFirstFrame);
        Assert.Equal(TimeSpan.FromSeconds(8), snapshot.PlaybackDuration);
        Assert.Equal(1, snapshot.RecoveryCount);
        Assert.Equal(0.2, snapshot.BufferingRatio, 3);
    }

    [Fact]
    public void Metrics_DoesNotCountPausedTimeAsPlayback()
    {
        var now = DateTimeOffset.Parse("2026-09-08T12:00:00Z");
        var metrics = new PlaybackMetrics(() => now);

        metrics.RecordAttempt();
        metrics.RecordState(Domain.Playback.PlaybackState.Playing);
        now = now.AddSeconds(5);
        metrics.RecordState(Domain.Playback.PlaybackState.Paused);
        now = now.AddMinutes(1);

        Assert.Equal(TimeSpan.FromSeconds(5), metrics.Snapshot().PlaybackDuration);
    }

    [Fact]
    public async Task HardenedEngine_UsesExponentialRetriesAndOpensCircuit()
    {
        var inner = new FailingPlaybackEngine();
        using var engine = new HardenedPlaybackEngine(
            inner,
            maxRetries: 2,
            retryBaseDelay: TimeSpan.Zero,
            circuitBreakerFailureThreshold: 3,
            circuitBreakerDuration: TimeSpan.FromMinutes(1));
        var request = new PlaybackRequest("https://stream.test/offline.m3u8");

        await Assert.ThrowsAsync<IOException>(() => engine.PlayAsync(request));
        var circuitError = await Assert.ThrowsAsync<InvalidOperationException>(() => engine.PlayAsync(request));

        Assert.Equal(3, inner.PlayCalls);
        Assert.Equal(2, engine.Metrics.Snapshot().RetryCount);
        Assert.Contains("Circuit breaker", circuitError.Message);
    }

    [Fact]
    public void HealthReport_ClassifiesLongBufferingAsSlow()
    {
        var metrics = new PlaybackMetricsSnapshot(1, 1, 1, 2, TimeSpan.FromSeconds(12), TimeSpan.FromSeconds(20));

        var report = PlaybackHealthReport.From(metrics, null, TimeSpan.FromSeconds(10));

        Assert.True(report.IsSlow);
        Assert.False(report.IsHealthy);
    }

    [Fact]
    public void HealthReport_ClassifiesErrorAsInvalid()
    {
        var metrics = new PlaybackMetricsSnapshot(1, 0, 0, 0, TimeSpan.Zero, TimeSpan.Zero);

        var report = PlaybackHealthReport.From(metrics, "stream inválido", TimeSpan.FromSeconds(10));

        Assert.True(report.IsInvalid);
        Assert.False(report.IsHealthy);
    }

    private sealed class FailingPlaybackEngine : IPlaybackEngine
    {
        public event EventHandler<PlaybackStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }
        public int PlayCalls { get; private set; }

        public Task PlayAsync(PlaybackRequest request, CancellationToken cancellationToken = default)
        {
            PlayCalls++;
            throw new IOException("stream offline");
        }

        public Task PauseAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
