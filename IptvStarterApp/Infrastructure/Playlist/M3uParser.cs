using System.Text.RegularExpressions;
using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Infrastructure.Playlist;

public sealed class M3uParser
{
    private const string DefaultChannelName = "Canal IPTV";
    private const string DefaultGroup = "General";
    private static readonly Regex AttributeRegex = new(
        @"(?<key>[A-Za-z0-9_-]+)\s*=\s*(?:(?:""(?<quoted>[^""]*)"")|(?<unquoted>[^\s,]+))",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<Channel> Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<Channel>();
        }

        var channels = new List<Channel>();
        var knownUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        ChannelMetadata? pending = null;

        using var reader = new StringReader(content);
        string? rawLine;
        while ((rawLine = reader.ReadLine()) is not null)
        {
            var line = rawLine.Trim().TrimStart('\uFEFF');
            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase))
            {
                pending = ParseMetadata(line);
                continue;
            }

            if (line.StartsWith('#'))
            {
                continue;
            }

            if (pending is null || !Uri.TryCreate(line, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                pending = null;
                continue;
            }

            if (!knownUrls.Add(uri.AbsoluteUri))
            {
                pending = null;
                continue;
            }

            var channel = new Channel(
                new MediaId(pending.Attributes.GetValueOrDefault("tvg-id") ?? uri.AbsoluteUri),
                pending.Name,
                uri,
                pending.Attributes.GetValueOrDefault("group-title") ?? DefaultGroup)
            {
                TvgId = pending.Attributes.GetValueOrDefault("tvg-id"),
                TvgName = pending.Attributes.GetValueOrDefault("tvg-name"),
                LogoUrl = pending.Attributes.GetValueOrDefault("tvg-logo"),
                Attributes = pending.Attributes
            };
            channels.Add(channel);
            pending = null;
        }

        return channels;
    }

    private static ChannelMetadata ParseMetadata(string line)
    {
        var metadataEnd = FindNameSeparator(line);
        var metadata = metadataEnd >= 0 ? line[..metadataEnd] : line;
        var name = metadataEnd >= 0 ? line[(metadataEnd + 1)..].Trim() : DefaultChannelName;
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in AttributeRegex.Matches(metadata))
        {
            var value = match.Groups["quoted"].Success
                ? match.Groups["quoted"].Value
                : match.Groups["unquoted"].Value;
            attributes[match.Groups["key"].Value] = value;
        }

        var tvgName = attributes.GetValueOrDefault("tvg-name");
        return new ChannelMetadata(
            string.IsNullOrWhiteSpace(name) ? tvgName ?? DefaultChannelName : name,
            attributes);
    }

    private static int FindNameSeparator(string line)
    {
        var quoted = false;
        for (var index = 0; index < line.Length; index++)
        {
            if (line[index] == '"')
            {
                quoted = !quoted;
            }
            else if (line[index] == ',' && !quoted)
            {
                return index;
            }
        }

        return -1;
    }

    private sealed record ChannelMetadata(string Name, Dictionary<string, string> Attributes);
}
