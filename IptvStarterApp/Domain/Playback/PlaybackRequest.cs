namespace IptvStarterApp.Domain.Playback;

public sealed record PlaybackRequest(
    string Url,
    string? Title = null,
    TimeSpan? ResumePosition = null)
{
    public bool IsHls => Url.Contains(".m3u8", StringComparison.OrdinalIgnoreCase);
}
