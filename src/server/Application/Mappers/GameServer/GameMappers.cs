using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.Publishers;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class GameMappers
{
    public static GameSlim ToSlim(this GameDb gameDb)
    {
        return new GameSlim
        {
            Id = gameDb.Id,
            FriendlyName = gameDb.FriendlyName,
            SteamName = gameDb.FriendlyName,
            SteamGameId = gameDb.SteamGameId,
            SteamToolId = gameDb.SteamToolId,
            DefaultGameProfileId = gameDb.DefaultGameProfileId,
            UrlBackground = gameDb.UrlBackground,
            UrlLogo = gameDb.UrlLogo,
            UrlLogoSmall = gameDb.UrlLogoSmall,
            UrlWebsite = gameDb.UrlWebsite,
            ControllerSupport = gameDb.ControllerSupport,
            DescriptionShort = gameDb.DescriptionShort,
            DescriptionLong = gameDb.DescriptionLong,
            DescriptionAbout = gameDb.DescriptionAbout,
            PriceInitial = gameDb.PriceInitial,
            PriceCurrent = gameDb.PriceCurrent,
            PriceDiscount = gameDb.PriceDiscount,
            MetaCriticScore = gameDb.MetaCriticScore,
            UrlMetaCriticPage = gameDb.UrlMetaCriticPage,
            RequirementsPcMinimum = gameDb.RequirementsPcMinimum,
            RequirementsPcRecommended = gameDb.RequirementsPcRecommended,
            RequirementsMacMinimum = gameDb.RequirementsMacMinimum,
            RequirementsMacRecommended = gameDb.RequirementsMacRecommended,
            RequirementsLinuxMinimum = gameDb.RequirementsLinuxMinimum,
            RequirementsLinuxRecommended = gameDb.RequirementsLinuxRecommended
        };
    }
    
    public static IEnumerable<GameSlim> ToSlims(this IEnumerable<GameDb> gameDbs)
    {
        return gameDbs.Select(ToSlim);
    }

    public static GameUpdate ToUpdate(this GameDb gameDb)
    {
        return new GameUpdate
        {
            Id = gameDb.Id,
            FriendlyName = gameDb.FriendlyName,
            SteamName = gameDb.SteamName,
            SteamGameId = gameDb.SteamGameId,
            SteamToolId = gameDb.SteamToolId,
            DefaultGameProfileId = gameDb.DefaultGameProfileId,
            UrlBackground = gameDb.UrlBackground,
            UrlLogo = gameDb.UrlLogo,
            UrlLogoSmall = gameDb.UrlLogoSmall,
            UrlWebsite = gameDb.UrlWebsite,
            ControllerSupport = gameDb.ControllerSupport,
            DescriptionShort = gameDb.DescriptionShort,
            DescriptionLong = gameDb.DescriptionLong,
            DescriptionAbout = gameDb.DescriptionAbout,
            PriceInitial = gameDb.PriceInitial,
            PriceCurrent = gameDb.PriceCurrent,
            PriceDiscount = gameDb.PriceDiscount,
            MetaCriticScore = gameDb.MetaCriticScore,
            UrlMetaCriticPage = gameDb.UrlMetaCriticPage,
            RequirementsPcMinimum = gameDb.RequirementsPcMinimum,
            RequirementsPcRecommended = gameDb.RequirementsPcRecommended,
            RequirementsMacMinimum = gameDb.RequirementsMacMinimum,
            RequirementsMacRecommended = gameDb.RequirementsMacRecommended,
            RequirementsLinuxMinimum = gameDb.RequirementsLinuxMinimum,
            RequirementsLinuxRecommended = gameDb.RequirementsLinuxRecommended,
            CreatedBy = gameDb.CreatedBy,
            CreatedOn = gameDb.CreatedOn,
            LastModifiedBy = gameDb.LastModifiedBy,
            LastModifiedOn = gameDb.LastModifiedOn
        };
    }

    public static GameFull ToFull(this GameDb gameDb)
    {
        return new GameFull
        {
            Id = gameDb.Id,
            FriendlyName = gameDb.FriendlyName,
            SteamName = gameDb.SteamName,
            SteamGameId = gameDb.SteamGameId,
            SteamToolId = gameDb.SteamToolId,
            DefaultGameProfile = new GameProfileSlim(),
            UrlBackground = gameDb.UrlBackground,
            UrlLogo = gameDb.UrlLogo,
            UrlLogoSmall = gameDb.UrlLogoSmall,
            UrlWebsite = gameDb.UrlWebsite,
            ControllerSupport = gameDb.ControllerSupport,
            DescriptionShort = gameDb.DescriptionShort,
            DescriptionLong = gameDb.DescriptionLong,
            DescriptionAbout = gameDb.DescriptionAbout,
            PriceInitial = gameDb.PriceInitial,
            PriceCurrent = gameDb.PriceCurrent,
            PriceDiscount = gameDb.PriceDiscount,
            MetaCriticScore = gameDb.MetaCriticScore,
            UrlMetaCriticPage = gameDb.UrlMetaCriticPage,
            RequirementsPcMinimum = gameDb.RequirementsPcMinimum,
            RequirementsPcRecommended = gameDb.RequirementsPcRecommended,
            RequirementsMacMinimum = gameDb.RequirementsMacMinimum,
            RequirementsMacRecommended = gameDb.RequirementsMacRecommended,
            RequirementsLinuxMinimum = gameDb.RequirementsLinuxMinimum,
            RequirementsLinuxRecommended = gameDb.RequirementsLinuxRecommended,
            CreatedBy = gameDb.CreatedBy,
            CreatedOn = gameDb.CreatedOn,
            LastModifiedBy = gameDb.LastModifiedBy,
            LastModifiedOn = gameDb.LastModifiedOn,
            Genres = new List<GameGenreSlim>(),
            Publishers = new List<PublisherSlim>(),
            Developers = new List<DeveloperSlim>()
        };
    }
}