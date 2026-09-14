using IptvStarterApp.Domain.Playback;

namespace IptvStarterApp.Infrastructure.Playback;

public sealed class PlaybackDiagnostics
{
    private readonly Action<string> _technicalLog;

    public PlaybackDiagnostics(Action<string>? technicalLog = null)
    {
        _technicalLog = technicalLog ?? (_ => { });
    }

    public void LogState(PlaybackState state, string? message = null)
    {
        var suffix = string.IsNullOrWhiteSpace(message) ? string.Empty : $" message={message}";
        _technicalLog($"[playback] state={state}{suffix}");
    }

    public void LogRetry(int attempt, string reason) =>
        _technicalLog($"[playback] retry attempt={attempt} reason={reason}");

    public void LogTimeout(TimeSpan timeout) =>
        _technicalLog($"[playback] timeout seconds={timeout.TotalSeconds:0.##}");
}
