using IptvStarterApp.Models;

namespace IptvStarterApp.Services
{
    public class MediaCatalogService
    {
        public List<ChannelItem> GetMockCatalog()
        {
            return new List<ChannelItem>
            {
                new ChannelItem { Name = "SBT", Url = "https://example.com/live/sbt.ts", Group = MediaCategory.Live },
                new ChannelItem { Name = "Globo", Url = "https://example.com/live/globo.ts", Group = MediaCategory.Live },
                new ChannelItem { Name = "Record", Url = "https://example.com/live/record.ts", Group = MediaCategory.Live },
                new ChannelItem { Name = "Filme 1", Url = "https://example.com/movies/filme1.ts", Group = MediaCategory.Movies },
                new ChannelItem { Name = "Filme 2", Url = "https://example.com/movies/filme2.ts", Group = MediaCategory.Movies },
                new ChannelItem { Name = "Série 1", Url = "https://example.com/series/serie1.ts", Group = MediaCategory.Series },
                new ChannelItem { Name = "Série 2", Url = "https://example.com/series/serie2.ts", Group = MediaCategory.Series },
                new ChannelItem { Name = "Favorito 1", Url = "https://example.com/favs/fav1.ts", Group = MediaCategory.Favorites }
            };
        }

        public IReadOnlyDictionary<string, List<ChannelItem>> GetGroupedCatalog()
        {
            var catalog = GetMockCatalog();
            var grouped = new Dictionary<string, List<ChannelItem>>
            {
                [MediaCategory.Live] = new(),
                [MediaCategory.Movies] = new(),
                [MediaCategory.Series] = new(),
                [MediaCategory.Favorites] = new()
            };

            foreach (var item in catalog)
            {
                if (grouped.ContainsKey(item.Group))
                {
                    grouped[item.Group].Add(item);
                }
            }

            return grouped;
        }
    }
}
