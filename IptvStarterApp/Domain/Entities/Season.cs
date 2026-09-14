using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Entities;

public sealed class Season
{
    public Season(
        SeasonId id,
        MediaId seriesId,
        int number,
        IEnumerable<Episode>? episodes = null,
        string? title = null)
    {
        if (id == default) throw new ArgumentException("O identificador da temporada é obrigatório.", nameof(id));
        if (seriesId == default) throw new ArgumentException("O identificador da série é obrigatório.", nameof(seriesId));
        if (number <= 0) throw new ArgumentOutOfRangeException(nameof(number), "O número da temporada deve ser maior que zero.");

        var materializedEpisodes = (episodes ?? Array.Empty<Episode>())
            .OrderBy(episode => episode.EpisodeNumber)
            .ToArray();

        if (materializedEpisodes.Any(episode => episode.SeriesId != seriesId || episode.SeasonId != id))
        {
            throw new ArgumentException("Todos os episódios devem pertencer à temporada e à série informadas.", nameof(episodes));
        }

        if (materializedEpisodes.Select(episode => episode.EpisodeNumber).Distinct().Count() != materializedEpisodes.Length)
        {
            throw new ArgumentException("A temporada não pode conter episódios com o mesmo número.", nameof(episodes));
        }

        Id = id;
        SeriesId = seriesId;
        Number = number;
        Title = string.IsNullOrWhiteSpace(title) ? $"Temporada {number}" : title.Trim();
        Episodes = materializedEpisodes;
    }

    public SeasonId Id { get; }
    public MediaId SeriesId { get; }
    public int Number { get; }
    public string Title { get; }
    public IReadOnlyList<Episode> Episodes { get; }
}