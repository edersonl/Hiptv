using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Interfaces;

public interface IPlaylistRepository
{
    Task<IReadOnlyList<Channel>> LoadAsync(
        string playlistUrl,
        CancellationToken cancellationToken = default);

    Task<Playlist?> GetByIdAsync(
        PlaylistId id,
        CancellationToken cancellationToken = default) =>
        Task.FromException<Playlist?>(new NotSupportedException("O adaptador ainda não oferece catálogo persistido de playlists."));

    Task<IReadOnlyList<Playlist>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromException<IReadOnlyList<Playlist>>(new NotSupportedException("O adaptador ainda não oferece catálogo persistido de playlists."));

    Task SaveAsync(Playlist playlist, CancellationToken cancellationToken = default) =>
        Task.FromException(new NotSupportedException("O adaptador ainda não oferece persistência de playlists."));
}
