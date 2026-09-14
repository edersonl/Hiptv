namespace IptvStarterApp.Domain.Playback;

public enum PlaybackState
{
    Idle,
    Buffering,
    Playing,
    Paused,
    Stopped,
    Completed,
    Error
}

public sealed class PlaybackStateChangedEventArgs : EventArgs
{
    public PlaybackStateChangedEventArgs(PlaybackState state, string? message = null)
    {
        State = state;
        Message = message;
    }

    public PlaybackState State { get; }
    public string? Message { get; }
}
