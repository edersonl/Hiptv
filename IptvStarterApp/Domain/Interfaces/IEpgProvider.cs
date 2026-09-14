namespace IptvStarterApp.Domain.Interfaces;

public interface IEpgProvider
{
    Task<IReadOnlyList<EpgEntry>> GetEntriesAsync(
        string channelId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}

public sealed record EpgEntry(
    string ChannelId,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? Description = null);
