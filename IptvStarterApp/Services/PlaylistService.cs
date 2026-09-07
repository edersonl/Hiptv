using System.Text.RegularExpressions;
using IptvStarterApp.Models;

namespace IptvStarterApp.Services
{
    public class PlaylistService
    {
        private readonly HttpClient _httpClient;

        public PlaylistService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<List<ChannelItem>> LoadAsync(string playlistUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(playlistUrl))
            {
                throw new ArgumentException("A URL da playlist M3U não pode estar vazia.", nameof(playlistUrl));
            }

            using var response = await _httpClient.GetAsync(playlistUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return Parse(content);
        }

        public static List<ChannelItem> Parse(string content)
        {
            var channels = new List<ChannelItem>();
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            ChannelItem? currentChannel = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase))
                {
                    currentChannel = new ChannelItem();

                    var split = line.Split(',', 2, StringSplitOptions.None);
                    var metadata = split[0];
                    currentChannel.Name = split.Length > 1 ? split[1].Trim() : "Canal IPTV";

                    var groupMatch = Regex.Match(metadata, @"group-title=""([^""]+)""", RegexOptions.IgnoreCase);
                    if (groupMatch.Success)
                    {
                        currentChannel.Group = groupMatch.Groups[1].Value;
                    }

                    continue;
                }

                if (line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                if (currentChannel == null)
                {
                    continue;
                }

                currentChannel.Url = line;
                channels.Add(currentChannel);
                currentChannel = null;
            }

            return channels;
        }
    }
}
