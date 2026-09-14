using IptvStarterApp.Domain.Interfaces;
using IptvStarterApp.Domain.Entities;

namespace IptvStarterApp.Infrastructure.Playlist;

public sealed class M3uPlaylistRepository : IPlaylistRepository
{
    private readonly HttpClient _httpClient;
    private readonly M3uParser _parser;

    public M3uPlaylistRepository(HttpClient httpClient, M3uParser? parser = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _parser = parser ?? new M3uParser();
    }

    public async Task<IReadOnlyList<Channel>> LoadAsync(
        string playlistUrl,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(playlistUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("A URL da playlist M3U deve ser HTTP ou HTTPS.", nameof(playlistUrl));
        }

        using var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return _parser.Parse(content);
    }
}
