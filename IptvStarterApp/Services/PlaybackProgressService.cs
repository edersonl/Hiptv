using Android.Content;
using System.Text.Json;
using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Services;

public sealed class PlaybackProgressService
{
    private const string PreferencesName = "HiptvPlayback";
    private const string Key = "playback_progress";

    private readonly ISharedPreferences? _preferences;

    public PlaybackProgressService(Context context)
    {
        _preferences =
            context.GetSharedPreferences(
                PreferencesName,
                FileCreationMode.Private);
    }

    public IReadOnlyList<PlaybackProgress> GetHistory()
    {
        var raw =
            _preferences?.GetString(Key, string.Empty);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<PlaybackProgress>();
        }

        try
        {
            return (JsonSerializer.Deserialize<List<StoredPlaybackProgress>>(raw) ?? new())
                .Select(item => item.ToDomain())
                .ToArray();
        }
        catch (JsonException)
        {
            return Array.Empty<PlaybackProgress>();
        }
    }

    public void SaveHistory(IEnumerable<PlaybackProgress> history)
    {
        var raw =
            JsonSerializer.Serialize(
                history.Select(
                    StoredPlaybackProgress.FromDomain));

        _preferences?
            .Edit()?
            .PutString(Key, raw)?
            .Apply();
    }

    private sealed record StoredPlaybackProgress(
        string MediaId,
        long PositionTicks,
        long? DurationTicks,
        DateTimeOffset LastPlayedAt,
        int PlayCount,
        string ProfileId)
    {
        public static StoredPlaybackProgress FromDomain(
            PlaybackProgress progress)
        {
            return new(
                progress.MediaId.ToString(),
                progress.Position.Ticks,
                progress.Duration?.Ticks,
                progress.LastPlayedAt,
                progress.PlayCount,
                progress.ProfileId);
        }

        public PlaybackProgress ToDomain()
        {
            return new PlaybackProgress(
                new MediaId(MediaId),
                new TimeSpan(PositionTicks),
                DurationTicks.HasValue
                    ? new TimeSpan(DurationTicks.Value)
                    : null,
                LastPlayedAt,
                PlayCount,
                ProfileId);
        }
    }
}