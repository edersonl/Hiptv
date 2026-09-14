namespace IptvStarterApp.Infrastructure.Playback;

public sealed record PlaybackHealthReport(
    bool IsHealthy,
    bool IsSlow,
    bool IsInvalid,
    int RetryCount,
    int ReconnectionCount,
    int BufferingEvents,
    TimeSpan TotalBuffering,
    TimeSpan PlaybackDuration,
    string? LastError)
{
    public static PlaybackHealthReport From(
        PlaybackMetricsSnapshot metrics,
        string? lastError,
        TimeSpan slowBufferingThreshold)
    {
        var isInvalid = !string.IsNullOrWhiteSpace(lastError) && metrics.PlayAttempts > 0;
        var isSlow = metrics.TotalBuffering >= slowBufferingThreshold;
        return new PlaybackHealthReport(
            !isInvalid && !isSlow,
            isSlow,
            isInvalid,
            metrics.RetryCount,
            metrics.ReconnectionCount,
            metrics.BufferingEvents,
            metrics.TotalBuffering,
            metrics.PlaybackDuration,
            lastError);
    }
}
