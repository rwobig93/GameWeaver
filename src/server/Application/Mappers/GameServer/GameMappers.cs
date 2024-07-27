using Application.Models.External.Steam;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Requests.GameServer.Game;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;

namespace Application.Mappers.GameServer;

public static class GameMappers
{
    public static GameSlim ToSlim(this GameDb gameDb)
    {
        return new GameSlim
        {
            Id = gameDb.Id,
            FriendlyName = gameDb.FriendlyName,
            SteamName = gameDb.SteamName,
            SteamGameId = gameDb.SteamGameId,
            SteamToolId = gameDb.SteamToolId,
            DefaultGameProfileId = gameDb.DefaultGameProfileId,
            LatestBuildVersion = gameDb.LatestBuildVersion,
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
            SupportsWindows = gameDb.SupportsWindows,
            SupportsLinux = gameDb.SupportsLinux,
            SupportsMac = gameDb.SupportsMac,
            SourceType = gameDb.SourceType,
            ManualFileRecordId = gameDb.ManualFileRecordId,
            ManualVersionUrlCheck = gameDb.ManualVersionUrlCheck,
            ManualVersionUrlDownload = gameDb.ManualVersionUrlDownload
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
            LatestBuildVersion = gameDb.LatestBuildVersion,
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
            SupportsWindows = gameDb.SupportsWindows,
            SupportsLinux = gameDb.SupportsLinux,
            SupportsMac = gameDb.SupportsMac,
            SourceType = gameDb.SourceType,
            ManualFileRecordId = gameDb.ManualFileRecordId,
            ManualVersionUrlCheck = gameDb.ManualVersionUrlCheck,
            ManualVersionUrlDownload = gameDb.ManualVersionUrlDownload
        };
    }

    public static GameUpdateRequest ToUpdate(this GameSlim game)
    {
        return new GameUpdateRequest
        {
            Id = game.Id,
            DefaultGameProfileId = game.DefaultGameProfileId,
            LatestBuildVersion = game.LatestBuildVersion,
            UrlBackground = game.UrlBackground,
            UrlLogo = game.UrlLogo,
            UrlLogoSmall = game.UrlLogoSmall,
            UrlWebsite = game.UrlWebsite,
            ControllerSupport = game.ControllerSupport,
            DescriptionShort = game.DescriptionShort,
            DescriptionLong = game.DescriptionLong,
            DescriptionAbout = game.DescriptionAbout,
            UrlMetaCriticPage = game.UrlMetaCriticPage,
            SupportsWindows = game.SupportsWindows,
            SupportsLinux = game.SupportsLinux,
            SupportsMac = game.SupportsMac,
            ManualFileRecordId = game.ManualFileRecordId,
            ManualVersionUrlCheck = game.ManualVersionUrlCheck,
            ManualVersionUrlDownload = game.ManualVersionUrlDownload
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
            LatestBuildVersion = gameDb.LatestBuildVersion,
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
            SupportsWindows = gameDb.SupportsWindows,
            SupportsLinux = gameDb.SupportsLinux,
            SupportsMac = gameDb.SupportsMac,
            SourceType = gameDb.SourceType,
            ManualFileRecordId = gameDb.ManualFileRecordId,
            ManualVersionUrlCheck = gameDb.ManualVersionUrlCheck,
            ManualVersionUrlDownload = gameDb.ManualVersionUrlDownload,
            Genres = [],
            Publishers = [],
            Developers = []
        };
    }

    public static GameCreate ToCreate(this GameCreateRequest request)
    {
        return new GameCreate
        {
            FriendlyName = request.Name,
            SteamGameId = request.SteamGameId,
            SteamToolId = request.SteamToolId,
            DescriptionShort = request.Description,
            SupportsWindows = request.SupportsWindows,
            SupportsLinux = request.SupportsLinux,
            SupportsMac = request.SupportsMac,
            SourceType = request.SourceType,
            ManualFileRecordId = request.ManualFileRecordId,
            ManualVersionUrlCheck = request.ManualVersionUrlCheck,
            ManualVersionUrlDownload = request.ManualVersionUrlDownload
        };
    }

    public static GameUpdate ToUpdate(this GameUpdateRequest request)
    {
        return new GameUpdate
        {
            Id = request.Id,
            FriendlyName = request.Name,
            DefaultGameProfileId = request.DefaultGameProfileId,
            LatestBuildVersion = request.LatestBuildVersion,
            UrlBackground = request.UrlBackground,
            UrlLogo = request.UrlLogo,
            UrlLogoSmall = request.UrlLogoSmall,
            UrlWebsite = request.UrlWebsite,
            ControllerSupport = request.ControllerSupport,
            DescriptionShort = request.DescriptionShort,
            DescriptionLong = request.DescriptionLong,
            DescriptionAbout = request.DescriptionAbout,
            UrlMetaCriticPage = request.UrlMetaCriticPage,
            SupportsWindows = request.SupportsWindows,
            SupportsLinux = request.SupportsLinux,
            SupportsMac = request.SupportsMac,
            ManualFileRecordId = request.ManualFileRecordId,
            ManualVersionUrlCheck = request.ManualVersionUrlCheck,
            ManualVersionUrlDownload = request.ManualVersionUrlDownload
        };
    }

    public static GameUpdate ToUpdate(this SteamAppDetailResponseJson? response, Guid gameId)
    {
        if (response is null)
        {
            return new GameUpdate {Id = gameId};
        }

        return new GameUpdate
        {
            Id = gameId,
            SteamName = response.Name,
            SteamGameId = response.Steam_AppId,
            UrlBackground = response.Background_Raw,
            UrlLogo = response.Header_Image,
            ControllerSupport = response.Controller_Support,
            DescriptionShort = response.Short_Description,
            DescriptionLong = response.Detailed_Description,
            DescriptionAbout = response.About_The_Game,
            PriceInitial = response.Price_Overview.Final_Formatted,
            PriceCurrent = response.Price_Overview.Final_Formatted,
            PriceDiscount = response.Price_Overview.Discount_Percent,
            // TODO: Steam api returns dict if there is a value or an empty list if there isn't a value
            // RequirementsPcMinimum = response.PC_Requirements.Minimum,
            // RequirementsPcRecommended = response.PC_Requirements.Recommended,
            // RequirementsMacMinimum = response.Mac_Requirements.Minimum,
            // RequirementsMacRecommended = response.Mac_Requirements.Recommended,
            // RequirementsLinuxMinimum = response.Linux_Requirements.Minimum,
            // RequirementsLinuxRecommended = response.Linux_Requirements.Recommended,
            SupportsWindows = response.Platforms.Windows,
            SupportsLinux = response.Platforms.Linux,
            SupportsMac = response.Platforms.Mac,
            SourceType = GameSource.Steam
        };
    }
}