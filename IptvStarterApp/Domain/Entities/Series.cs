using IptvStarterApp.Domain.ValueObjects;
using IptvStarterApp.Domain.Enums;

namespace IptvStarterApp.Domain.Entities;

public sealed class Series : MediaItem
{
    public Series(MediaId id, string name, IEnumerable<Season>? seasons = null, string group = "Séries")
        : base(id, name, MediaType.Series, group: group)
    {
        var materializedSeasons = (seasons ?? Array.Empty<Season>())
            .OrderBy(season => season.Number)
            .ToArray();

        if (materializedSeasons.Any(season => season.SeriesId != id))
        {
            throw new ArgumentException("Todas as temporadas devem pertencer à série.", nameof(seasons));
        }

        if (materializedSeasons.Select(season => season.Number).Distinct().Count() != materializedSeasons.Length)
        {
            throw new ArgumentException("A série não pode conter temporadas com o mesmo número.", nameof(seasons));
        }

        Seasons = materializedSeasons;
    }

    public ContentType ContentType => ContentType.Serialized;
    public IReadOnlyList<Season> Seasons { get; }
    public int TotalSeasons => Seasons.Count;
    public IReadOnlyList<Episode> Episodes => Seasons.SelectMany(season => season.Episodes).ToArray();
}
