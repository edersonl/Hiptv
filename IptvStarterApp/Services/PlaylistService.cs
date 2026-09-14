using IptvStarterApp.Infrastructure.Playlist;
using IptvStarterApp.Models;

namespace IptvStarterApp.Services
{
    public class PlaylistService
    {
        private readonly M3uPlaylistRepository _repository;
        private static readonly M3uParser Parser = new();

        public PlaylistService(HttpClient? httpClient = null)
        {
            _repository = new M3uPlaylistRepository(httpClient ?? new HttpClient(), Parser);
        }

        public async Task<List<ChannelItem>> LoadAsync(string playlistUrl, CancellationToken cancellationToken = default)
        {
            var channels = await _repository.LoadAsync(playlistUrl, cancellationToken);
            return channels.Select(ToLegacyChannel).ToList();
        }

        public static List<ChannelItem> Parse(string content)
        {
            return Parser.Parse(content).Select(ToLegacyChannel).ToList();
        }

        private static ChannelItem ToLegacyChannel(Domain.Entities.Channel channel)
        {
            return new ChannelItem
            {
                Id = channel.Id.Value,
                Name = channel.Name,
                Url = channel.Source.AbsoluteUri,
                Group = channel.Group,
                TvgId = channel.TvgId,
                TvgName = channel.TvgName,
                TvgLogo = channel.LogoUrl,
                Attributes = channel.Attributes
            };
        }
    }
}
