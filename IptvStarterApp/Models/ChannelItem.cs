namespace IptvStarterApp.Models
{
    public class ChannelItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Group { get; set; } = "General";
        public string? TvgId { get; set; }
        public string? TvgName { get; set; }
        public string? TvgLogo { get; set; }
        public IReadOnlyDictionary<string, string> Attributes { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public override string ToString()
        {
            return Name;
        }
    }
}
