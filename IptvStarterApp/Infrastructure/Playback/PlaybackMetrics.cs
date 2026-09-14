namespace IptvStarterApp.Infrastructure.Playback;

public sealed class PlaybackMetrics
{
    private readonly object _sync = new();
    private readonly Func<DateTimeOffset> _utcNow;
    private DateTimeOffset? _requestedAt;
    private DateTimeOffset? _playingStartedAt;
    private DateTimeOffset? _bufferingStartedAt;

    public PlaybackMetrics(Func<DateTimeOffset>? utcNow = null)
    {
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public int PlayAttempts { get; private set; }
    public int RetryCount { get; private set; }
    public int ReconnectionCount { get; private set; }
    public int BufferingEvents { get; private set; }
    public TimeSpan TotalBuffering { get; private set; }
    public TimeSpan PlaybackDuration { get; private set; }

    public TimeSpan StartupTime { get; private set; }
    public TimeSpan TimeToFirstFrame { get; private set; }

    public void RecordAttempt()
    {
        lock (_sync)
        {
            PlayAttempts++;
            _requestedAt ??= _utcNow();
        }
    }

    public void RecordRetry() { lock (_sync) RetryCount++; }
    public void RecordReconnection() { lock (_sync) ReconnectionCount++; }

    public void RecordState(Domain.Playback.PlaybackState state)
    {
        lock (_sync)
        {
            var now = _utcNow();
            if (state == Domain.Playback.PlaybackState.Buffering && _bufferingStartedAt is null)
            {
                ClosePlayback(now);
                BufferingEvents++;
                _bufferingStartedAt = now;
            }
            else if (state == Domain.Playback.PlaybackState.Playing)
            {
                CloseBuffering(now);
                if (_requestedAt is { } requested && TimeToFirstFrame == TimeSpan.Zero)
                {
                    StartupTime = now - requested;
                    TimeToFirstFrame = StartupTime;
                }

                _playingStartedAt ??= now;
            }
            else if (state is Domain.Playback.PlaybackState.Paused or Domain.Playback.PlaybackState.Stopped or Domain.Playback.PlaybackState.Completed or Domain.Playback.PlaybackState.Error)
            {
                CloseBuffering(now);
                ClosePlayback(now);
            }
        }
    }

    public PlaybackMetricsSnapshot Snapshot()
    {
        lock (_sync)
        {
            var now = _utcNow();
            var totalBuffering = TotalBuffering + ElapsedSince(_bufferingStartedAt, now);
            var playbackDuration = PlaybackDuration + ElapsedSince(_playingStartedAt, now);
            return new PlaybackMetricsSnapshot(PlayAttempts, RetryCount, ReconnectionCount, BufferingEvents, totalBuffering, playbackDuration)
            {
                StartupTime = StartupTime,
                TimeToFirstFrame = TimeToFirstFrame
            };
        }
    }

    private static TimeSpan ElapsedSince(DateTimeOffset? startedAt, DateTimeOffset now) =>
        startedAt is { } started ? now - started : TimeSpan.Zero;

    private void CloseBuffering(DateTimeOffset now)
    {
        if (_bufferingStartedAt is { } started)
        {
            TotalBuffering += now - started;
            _bufferingStartedAt = null;
        }
    }

    private void ClosePlayback(DateTimeOffset now)
    {
        if (_playingStartedAt is { } started)
        {
            PlaybackDuration += now - started;
            _playingStartedAt = null;
        }
    }
}

public sealed record PlaybackMetricsSnapshot(
    int PlayAttempts,
    int RetryCount,
    int ReconnectionCount,
    int BufferingEvents,
    TimeSpan TotalBuffering,
    TimeSpan PlaybackDuration)
{
    public TimeSpan StartupTime { get; init; }
    public TimeSpan TimeToFirstFrame { get; init; }
    public int RecoveryCount => ReconnectionCount;
    public double BufferingRatio =>
        TotalBuffering + PlaybackDuration == TimeSpan.Zero
            ? 0
            : TotalBuffering.TotalMilliseconds / (TotalBuffering + PlaybackDuration).TotalMilliseconds;
}
