using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class GameGenreMappers
{
    public static GameGenreSlim ToSlim(this GameGenreDb genreDb)
    {
        return new GameGenreSlim
        {
            Id = genreDb.Id,
            GameId = genreDb.GameId,
            Name = genreDb.Name,
            Description = genreDb.Description
        };
    }
    
    public static IEnumerable<GameGenreSlim> ToSlims(this IEnumerable<GameGenreDb> genreDbs)
    {
        return genreDbs.Select(ToSlim);
    }
    
    public static GameGenreFull ToFull(this GameGenreDb genreDb)
    {
        return new GameGenreFull
        {
            Id = genreDb.Id,
            GameId = genreDb.GameId,
            Name = genreDb.Name,
            Description = genreDb.Description,
            Games = []
        };
    }
}