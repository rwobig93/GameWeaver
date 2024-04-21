using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class DeveloperMappers
{
    public static DeveloperSlim ToSlim(this DeveloperDb developerDb)
    {
        return new DeveloperSlim
        {
            Id = developerDb.Id,
            GameId = developerDb.GameId,
            Name = developerDb.Name
        };
    }
    
    public static IEnumerable<DeveloperSlim> ToSlims(this IEnumerable<DeveloperDb> developerDbs)
    {
        return developerDbs.Select(x => x.ToSlim()).ToList();
    }
    
    public static DeveloperFull ToFull(this DeveloperDb developerDb)
    {
        return new DeveloperFull
        {
            Id = developerDb.Id,
            GameId = developerDb.GameId,
            Name = developerDb.Name,
            Games = []
        };
    }
    
    public static IEnumerable<DeveloperFull> ToFulls(this IEnumerable<DeveloperDb> developerDbs)
    {
        return developerDbs.Select(x => x.ToFull()).ToList();
    }
}