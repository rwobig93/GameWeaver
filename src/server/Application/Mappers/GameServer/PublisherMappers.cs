using Application.Models.GameServer.Game;
using Application.Models.GameServer.Publishers;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class PublisherMappers
{
    public static PublisherSlim ToSlim(this PublisherDb publisherDb)
    {
        return new PublisherSlim
        {
            Id = publisherDb.Id,
            GameId = publisherDb.GameId,
            Name = publisherDb.Name
        };
    }
    
    public static IEnumerable<PublisherSlim> ToSlims(this IEnumerable<PublisherDb> publisherDbs)
    {
        return publisherDbs.Select(x => x.ToSlim()).ToList();
    }
    
    public static PublisherFull ToFull(this PublisherDb publisherDb)
    {
        return new PublisherFull
        {
            Id = publisherDb.Id,
            GameId = publisherDb.GameId,
            Name = publisherDb.Name,
            Games = []
        };
    }
    
    public static IEnumerable<PublisherFull> ToFulls(this IEnumerable<PublisherDb> publisherDbs)
    {
        return publisherDbs.Select(x => x.ToFull()).ToList();
    }
}