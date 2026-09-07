namespace IptvStarterApp.Models
{
    public class ChannelItem
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Group { get; set; } = "General";

        public override string ToString()
        {
            return Name;
        }
    }
}
