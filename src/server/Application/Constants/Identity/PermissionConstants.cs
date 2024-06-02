namespace Application.Constants.Identity;

public static class PermissionConstants
{
    // Built-in Permissions are the following format: Permissions.Group.Name.Access => Permissions.Identity.Users.View
    // Dynamic Permissions are the following format: Dynamic.Group.Id.AccessLevel => Dynamic.ServiceAccounts.<Guid>.Admin
    
    public static class Users
    {
        public const string View = "Permissions.Identity.Users.View";
        public const string Create = "Permissions.Identity.Users.Create";
        public const string Edit = "Permissions.Identity.Users.Edit";
        public const string Delete = "Permissions.Identity.Users.Delete";
        public const string Export = "Permissions.Identity.Users.Export";
        public const string Enable = "Permissions.Identity.Users.Enable";
        public const string Disable = "Permissions.Identity.Users.Disable";
        public const string ResetPassword = "Permissions.Identity.Users.ResetPassword";
        public const string ViewExtAttrs = "Permissions.Identity.Users.ExtendedAttrView";
        public const string ChangeEmail = "Permissions.Identity.Users.ChangeEmail";
        public const string AdminEmail = "Permissions.Identity.Users.AdminEmail";
    }

    public static class ServiceAccounts
    {
        public const string View = "Permissions.Identity.Service.View";
        public const string Admin = "Permissions.Identity.Service.Admin";
    }

    public static class Roles
    {
        public const string View = "Permissions.Identity.Roles.View";
        public const string Create = "Permissions.Identity.Roles.Create";
        public const string Edit = "Permissions.Identity.Roles.Edit";
        public const string Delete = "Permissions.Identity.Roles.Delete";
        public const string Add = "Permissions.Identity.Roles.Add";
        public const string Remove = "Permissions.Identity.Roles.Remove";
        public const string Export = "Permissions.Identity.Roles.Export";
    }

    public static class Permissions
    {
        public const string View = "Permissions.Identity.Permissions.View";
        public const string Add = "Permissions.Identity.Permissions.Add";
        public const string Remove = "Permissions.Identity.Permissions.Remove";
    }
    
    public static class Preferences
    {
        public const string ChangeTheme = "Permissions.Identity.Preferences.ChangeTheme";
    }

    public static class Jobs
    {
        public const string View = "Permissions.System.Jobs.View";
        public const string Status = "Permissions.System.Jobs.Status";
    }

    public static class Api
    {
        public const string View = "Permissions.System.Api.View";
        public const string GenerateToken = "Permissions.System.Api.GenerateToken";
    }

    public static class Audit
    {
        public const string View = "Permissions.System.Audit.View";
        public const string Export = "Permissions.System.Audit.Export";
        public const string Search = "Permissions.System.Audit.Search";
        public const string DeleteOld = "Permissions.System.Audit.DeleteOld";
    }

    public static class Developer
    {
        public const string Dev = "Permissions.System.Developer.Dev";
        public const string Contributor = "Permissions.System.Developer.Contributor";
        public const string Tester = "Permissions.System.Developer.Tester";
    }

    public static class Hosts
    {
        public const string CreateRegistration = "Permissions.GameServer.Hosts.CreateRegistration";
        public const string Register = "Permissions.GameServer.Hosts.Register";
        public const string CheckIn = "Permissions.GameServer.Hosts.CheckIn";
        public const string WorkUpdate = "Permissions.GameServer.Hosts.WorkUpdate";
        public const string GetAll = "Permissions.GameServer.Hosts.GetAll";
        public const string GetAllPaginated = "Permissions.GameServer.Hosts.GetAllPaginated";
        public const string Get = "Permissions.GameServer.Hosts.Get";
        public const string Create = "Permissions.GameServer.Hosts.Create";
        public const string Update = "Permissions.GameServer.Hosts.Update";
        public const string Delete = "Permissions.GameServer.Hosts.Delete";
        public const string Search = "Permissions.GameServer.Hosts.Search";
        public const string SearchPaginated = "Permissions.GameServer.Hosts.SearchPaginated";
        public const string GetAllRegistrations = "Permissions.GameServer.Hosts.GetAllRegistrations";
        public const string GetAllRegistrationsPaginated = "Permissions.GameServer.Hosts.GetAllRegistrationsPaginated";
        public const string GetAllRegistrationsActive = "Permissions.GameServer.Hosts.GetAllRegistrationsActive";
        public const string GetAllRegistrationsInActive = "Permissions.GameServer.Hosts.GetAllRegistrationsInActive";
        public const string GetRegistrationsCount = "Permissions.GameServer.Hosts.GetRegistrationCount";
        public const string UpdateRegistration = "Permissions.GameServer.Hosts.UpdateRegistration";
        public const string SearchRegistrations = "Permissions.GameServer.Hosts.SearchRegistrations";
        public const string SearchRegistrationsPaginated = "Permissions.GameServer.Hosts.SearchRegistrationsPaginated";
        public const string GetAllCheckins = "Permissions.GameServer.Hosts.GetAllCheckins";
        public const string GetAllCheckinsPaginated = "Permissions.GameServer.Hosts.GetAllCheckinsPaginated";
        public const string GetCheckinCount = "Permissions.GameServer.Hosts.GetCheckinCount";
        public const string GetCheckin = "Permissions.GameServer.Hosts.GetCheckin";
        public const string GetCheckinByHost = "Permissions.GameServer.Hosts.GetCheckinByHost";
        public const string DeleteOldCheckins = "Permissions.GameServer.Hosts.DeleteOldCheckins";
        public const string SearchCheckins = "Permissions.GameServer.Hosts.SearchCheckins";
        public const string SearchCheckinsPaginated = "Permissions.GameServer.Hosts.SearchCheckinsPaginated";
        public const string GetAllWeaverWorkPaginated = "Permissions.GameServer.Hosts.GetAllWeaverWorkPaginated";
        public const string GetWeaverWorkCount = "Permissions.GameServer.Hosts.GetWeaverWorkCount";
        public const string GetWeaverWork = "Permissions.GameServer.Hosts.GetWeaverWork";
        public const string CreateWeaverWork = "Permissions.GameServer.Hosts.CreateWeaverWork";
        public const string UpdateWeaverWork = "Permissions.GameServer.Hosts.UpdateWeaverWork";
        public const string DeleteWeaverWork = "Permissions.GameServer.Hosts.DeleteWeaverWork";
        public const string SearchWeaverWork = "Permissions.GameServer.Hosts.SearchWeaverWork";
        public const string SearchWeaverWorkPaginated = "Permissions.GameServer.Hosts.SearchWeaverWorkPaginated";
    }

    public static class Gameserver
    {
        public const string GetAllPaginated = "Permissions.GameServer.Gameserver.GetAllPaginated";
        public const string GetCount = "Permissions.GameServer.Gameserver.GetCount";
        public const string GetById = "Permissions.GameServer.Gameserver.GetById";
        public const string GetByServerName = "Permissions.GameServer.Gameserver.GetByServerName";
        public const string GetByGameId = "Permissions.GameServer.Gameserver.GetByGameId";
        public const string GetByGameProfileId = "Permissions.GameServer.Gameserver.GetByGameProfileId";
        public const string GetByHostId = "Permissions.GameServer.Gameserver.GetByHostId";
        public const string GetByOwnerId = "Permissions.GameServer.Gameserver.GetByOwnerId";
        public const string Create = "Permissions.GameServer.Gameserver.Create";
        public const string Update = "Permissions.GameServer.Gameserver.Update";
        public const string Delete = "Permissions.GameServer.Gameserver.Delete";
        public const string Search = "Permissions.GameServer.Gameserver.Search";
        public const string GetAllConfigurationItemsPaginated = "Permissions.GameServer.Gameserver.GetAllConfigurationItemsPaginated";
        public const string GetConfigurationItemsCount = "Permissions.GameServer.Gameserver.GetConfigurationItemsCount";
        public const string GetConfigurationItemById = "Permissions.GameServer.Gameserver.GetConfigurationItemById";
        public const string GetConfigurationItemsByGameProfileId = "Permissions.GameServer.Gameserver.GetConfigurationItemsByGameProfileId";
        public const string CreateConfigurationItem = "Permissions.GameServer.Gameserver.CreateConfigurationItem";
        public const string UpdateConfigurationItem = "Permissions.GameServer.Gameserver.UpdateConfigurationItem";
        public const string DeleteConfigurationItem = "Permissions.GameServer.Gameserver.DeleteConfigurationItem";
        public const string SearchConfigurationItems = "Permissions.GameServer.Gameserver.SearchConfigurationItems";
        public const string GetAllLocalResourcesPaginated = "Permissions.GameServer.Gameserver.GetAllLocalResourcesPaginated";
        public const string GetLocalResourcesCount = "Permissions.GameServer.Gameserver.GetLocalResourcesCount";
        public const string GetLocalResourceById = "Permissions.GameServer.Gameserver.GetLocalResourceById";
        public const string GetLocalResourcesByGameProfileId = "Permissions.GameServer.Gameserver.GetLocalResourcesByGameProfileId";
        public const string GetLocalResourcesByGameServerId = "Permissions.GameServer.Gameserver.GetLocalResourcesByGameServerId";
        public const string CreateLocalResource = "Permissions.GameServer.Gameserver.CreateLocalResource";
        public const string UpdateLocalResource = "Permissions.GameServer.Gameserver.UpdateLocalResource";
        public const string DeleteLocalResource = "Permissions.GameServer.Gameserver.DeleteLocalResource";
        public const string UpdateLocalResourceOnGameServer = "Permissions.GameServer.Gameserver.UpdateLocalResourceOnGameServer";
        public const string UpdateAllLocalResourcesOnGameServer = "Permissions.GameServer.Gameserver.UpdateAllLocalResourcesOnGameServer";
        public const string SearchLocalResource = "Permissions.GameServer.Gameserver.SearchLocalResource";
        public const string GetAllGameProfilesPaginated = "Permissions.GameServer.Gameserver.GetAllGameProfilesPaginated";
        public const string GetGameProfileCount = "Permissions.GameServer.Gameserver.GetGameProfileCount";
        public const string GetGameProfileById = "Permissions.GameServer.Gameserver.GetGameProfileById";
        public const string GetGameProfileByFriendlyName = "Permissions.GameServer.Gameserver.GetGameProfileByFriendlyName";
        public const string GetGameProfilesByGameId = "Permissions.GameServer.Gameserver.GetGameProfilesByGameId";
        public const string GetGameProfilesByOwnerId = "Permissions.GameServer.Gameserver.GetGameProfilesByOwnerId";
        public const string GetGameProfilesByServerProcessName = "Permissions.GameServer.Gameserver.GetGameProfilesByServerProcessName";
        public const string CreateGameProfile = "Permissions.GameServer.Gameserver.CreateGameProfile";
        public const string UpdateGameProfile = "Permissions.GameServer.Gameserver.UpdateGameProfile";
        public const string DeleteGameProfile = "Permissions.GameServer.Gameserver.DeleteGameProfile";
        public const string SearchGameProfiles = "Permissions.GameServer.Gameserver.SearchGameProfiles";
        public const string GetAllModsPaginated = "Permissions.GameServer.Gameserver.GetAllModsPaginated";
        public const string GetModCount = "Permissions.GameServer.Gameserver.GetModCount";
        public const string GetModById = "Permissions.GameServer.Gameserver.GetModById";
        public const string GetModByCurrentHash = "Permissions.GameServer.Gameserver.GetModByCurrentHash";
        public const string GetModsByFriendlyName = "Permissions.GameServer.Gameserver.GetModsByFriendlyName";
        public const string GetModsByGameId = "Permissions.GameServer.Gameserver.GetModsByGameId";
        public const string GetModsBySteamGameId = "Permissions.GameServer.Gameserver.GetModsBySteamGameId";
        public const string GetModBySteamId = "Permissions.GameServer.Gameserver.GetModBySteamId";
        public const string GetModsBySteamToolId = "Permissions.GameServer.Gameserver.GetModsBySteamToolId";
        public const string CreateMod = "Permissions.GameServer.Gameserver.CreateMod";
        public const string UpdateMod = "Permissions.GameServer.Gameserver.UpdateMod";
        public const string DeleteMod = "Permissions.GameServer.Gameserver.DeleteMod";
        public const string SearchMods = "Permissions.GameServer.Gameserver.SearchMods";
        public const string StartServer = "Permissions.GameServer.Gameserver.StartServer";
        public const string StopServer = "Permissions.GameServer.Gameserver.StopServer";
        public const string RestartServer = "Permissions.GameServer.Gameserver.RestartServer";
    }

    public static class Game
    {
        public const string GetAllPaginated = "Permissions.GameServer.Game.GetAllPaginated";
        public const string GetCount = "Permissions.GameServer.Game.GetCount";
        public const string Get = "Permissions.GameServer.Game.Get";
        public const string Create = "Permissions.GameServer.Game.Create";
        public const string Update = "Permissions.GameServer.Game.Update";
        public const string Delete = "Permissions.GameServer.Game.Delete";
        public const string Search = "Permissions.GameServer.Game.Search";
        public const string GetAllDevelopersPaginated = "Permissions.GameServer.Game.GetAllDevelopersPaginated";
        public const string GetDevelopersCount = "Permissions.GameServer.Game.GetDevelopersCount";
        public const string GetDeveloper = "Permissions.GameServer.Game.GetDeveloper";
        public const string GetDevelopers = "Permissions.GameServer.Game.GetDevelopers";
        public const string CreateDeveloper = "Permissions.GameServer.Game.CreateDeveloper";
        public const string DeleteDeveloper = "Permissions.GameServer.Game.DeleteDeveloper";
        public const string SearchDevelopers = "Permissions.GameServer.Game.SearchDevelopers";
        public const string GetAllPublishersPaginated = "Permissions.GameServer.Game.GetAllPublishersPaginated";
        public const string GetPublishersCount = "Permissions.GameServer.Game.GetPublishersCount";
        public const string GetPublisher = "Permissions.GameServer.Game.GetPublisher";
        public const string GetPublishers = "Permissions.GameServer.Game.GetPublishers";
        public const string CreatePublisher = "Permissions.GameServer.Game.CreatePublisher";
        public const string DeletePublisher = "Permissions.GameServer.Game.DeletePublisher";
        public const string SearchPublishers = "Permissions.GameServer.Game.SearchPublishers";
        public const string GetAllGameGenresPaginated = "Permissions.GameServer.Game.GetAllGameGenresPaginated";
        public const string GetGameGenresCount = "Permissions.GameServer.Game.GetGameGenresCount";
        public const string GetGameGenre = "Permissions.GameServer.Game.GetGameGenre";
        public const string GetGameGenres = "Permissions.GameServer.Game.GetGameGenres";
        public const string CreateGameGenre = "Permissions.GameServer.Game.CreateGameGenre";
        public const string DeleteGameGenre = "Permissions.GameServer.Game.DeleteGameGenre";
        public const string SearchGameGenres = "Permissions.GameServer.Game.SearchGameGenres";
    }

    public static class Network
    {
        public const string GameserverConnectable = "Permissions.GameServer.Network.GameserverConnectable";
        public const string IsPortOpen = "Permissions.GameServer.Network.IsPortOpen";
    }
}
