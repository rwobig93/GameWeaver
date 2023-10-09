using Application.Models.GameServer.Mod;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class ModMappers
{
    public static ModSlim ToSlim(this ModDb modDb)
    {
        return new ModSlim
        {
            Id = modDb.Id,
            GameId = modDb.GameId,
            SteamGameId = modDb.SteamGameId,
            SteamToolId = modDb.SteamToolId,
            SteamId = modDb.SteamId,
            FriendlyName = modDb.FriendlyName,
            CurrentHash = modDb.CurrentHash,
            CreatedBy = modDb.CreatedBy,
            CreatedOn = modDb.CreatedOn,
            LastModifiedBy = modDb.LastModifiedBy,
            LastModifiedOn = modDb.LastModifiedOn,
            IsDeleted = modDb.IsDeleted,
            DeletedOn = modDb.DeletedOn
        };
    }
    
    public static IEnumerable<ModSlim> ToSlims(this IEnumerable<ModDb> modDbs)
    {
        return modDbs.Select(ToSlim);
    }

    public static ModUpdate ToUpdate(this ModDb modDb)
    {
        return new ModUpdate
        {
            Id = modDb.Id,
            GameId = modDb.GameId,
            SteamGameId = modDb.SteamGameId,
            SteamToolId = modDb.SteamToolId,
            SteamId = modDb.SteamId,
            FriendlyName = modDb.FriendlyName,
            CurrentHash = modDb.CurrentHash,
            CreatedBy = modDb.CreatedBy,
            CreatedOn = modDb.CreatedOn,
            LastModifiedBy = modDb.LastModifiedBy,
            LastModifiedOn = modDb.LastModifiedOn,
            IsDeleted = modDb.IsDeleted,
            DeletedOn = modDb.DeletedOn
        };
    }
}