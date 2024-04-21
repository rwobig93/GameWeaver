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
        public const string GetAllPaginated = "PermissionConstants.Gameserver.Gameserver.GetAllPaginated";
        public const string GetCount = "PermissionConstants.Gameserver.Gameserver.GetCount";
        public const string GetById = "PermissionConstants.Gameserver.Gameserver.GetById";
        public const string GetByServerName = "PermissionConstants.Gameserver.Gameserver.GetByServerName";
        public const string GetByGameId = "PermissionConstants.Gameserver.Gameserver.GetByGameId";
        public const string GetByGameProfileId = "PermissionConstants.Gameserver.Gameserver.GetByGameProfileId";
        public const string GetByHostId = "PermissionConstants.Gameserver.Gameserver.GetByHostId";
        public const string GetByOwnerId = "PermissionConstants.Gameserver.Gameserver.GetByOwnerId";
        public const string Create = "PermissionConstants.Gameserver.Gameserver.Create";
        public const string Update = "PermissionConstants.Gameserver.Gameserver.Update";
        public const string Delete = "PermissionConstants.Gameserver.Gameserver.Delete";
        public const string Search = "PermissionConstants.Gameserver.Gameserver.Search";
        public const string GetAllConfigurationItemsPaginated = "PermissionConstants.Gameserver.Gameserver.GetAllConfigurationItemsPaginated";
        public const string GetConfigurationItemsCount = "PermissionConstants.Gameserver.Gameserver.GetConfigurationItemsCount";
        public const string GetConfigurationItemById = "PermissionConstants.Gameserver.Gameserver.GetConfigurationItemById";
        public const string GetConfigurationItemsByGameProfileId = "PermissionConstants.Gameserver.Gameserver.GetConfigurationItemsByGameProfileId";
        public const string CreateConfigurationItem = "PermissionConstants.Gameserver.Gameserver.CreateConfigurationItem";
        public const string UpdateConfigurationItem = "PermissionConstants.Gameserver.Gameserver.UpdateConfigurationItem";
        public const string DeleteConfigurationItem = "PermissionConstants.Gameserver.Gameserver.DeleteConfigurationItem";
        public const string SearchConfigurationItems = "PermissionConstants.Gameserver.Gameserver.SearchConfigurationItems";
        public const string GetAllLocalResourcesPaginated = "PermissionConstants.Gameserver.Gameserver.GetAllLocalResourcesPaginated";
        public const string GetLocalResourcesCount = "PermissionConstants.Gameserver.Gameserver.GetLocalResourcesCount";
        public const string GetLocalResourceById = "PermissionConstants.Gameserver.Gameserver.GetLocalResourceById";
        public const string GetLocalResourcesByGameProfileId = "PermissionConstants.Gameserver.Gameserver.GetLocalResourcesByGameProfileId";
        public const string GetLocalResourcesByGameServerId = "PermissionConstants.Gameserver.Gameserver.GetLocalResourcesByGameServerId";
        public const string CreateLocalResource = "PermissionConstants.Gameserver.Gameserver.CreateLocalResource";
        public const string UpdateLocalResource = "PermissionConstants.Gameserver.Gameserver.UpdateLocalResource";
        public const string DeleteLocalResource = "PermissionConstants.Gameserver.Gameserver.DeleteLocalResource";
        public const string SearchLocalResource = "PermissionConstants.Gameserver.Gameserver.SearchLocalResource";
        public const string GetAllGameProfilesPaginated = "PermissionConstants.Gameserver.Gameserver.GetAllGameProfilesPaginated";
        public const string GetGameProfileCount = "PermissionConstants.Gameserver.Gameserver.GetGameProfileCount";
        public const string GetGameProfileById = "PermissionConstants.Gameserver.Gameserver.GetGameProfileById";
        public const string GetGameProfileByFriendlyName = "PermissionConstants.Gameserver.Gameserver.GetGameProfileByFriendlyName";
        public const string GetGameProfilesByGameId = "PermissionConstants.Gameserver.Gameserver.GetGameProfilesByGameId";
        public const string GetGameProfilesByOwnerId = "PermissionConstants.Gameserver.Gameserver.GetGameProfilesByOwnerId";
        public const string GetGameProfilesByServerProcessName = "PermissionConstants.Gameserver.Gameserver.GetGameProfilesByServerProcessName";
        public const string CreateGameProfile = "PermissionConstants.Gameserver.Gameserver.CreateGameProfile";
        public const string UpdateGameProfile = "PermissionConstants.Gameserver.Gameserver.UpdateGameProfile";
        public const string DeleteGameProfile = "PermissionConstants.Gameserver.Gameserver.DeleteGameProfile";
        public const string SearchGameProfiles = "PermissionConstants.Gameserver.Gameserver.SearchGameProfiles";
        public const string GetAllModsPaginated = "PermissionConstants.Gameserver.Gameserver.GetAllModsPaginated";
        public const string GetModCount = "PermissionConstants.Gameserver.Gameserver.GetModCount";
        public const string GetModById = "PermissionConstants.Gameserver.Gameserver.GetModById";
        public const string GetModByCurrentHash = "PermissionConstants.Gameserver.Gameserver.GetModByCurrentHash";
        public const string GetModsByFriendlyName = "PermissionConstants.Gameserver.Gameserver.GetModsByFriendlyName";
        public const string GetModsByGameId = "PermissionConstants.Gameserver.Gameserver.GetModsByGameId";
        public const string GetModsBySteamGameId = "PermissionConstants.Gameserver.Gameserver.GetModsBySteamGameId";
        public const string GetModBySteamId = "PermissionConstants.Gameserver.Gameserver.GetModBySteamId";
        public const string GetModsBySteamToolId = "PermissionConstants.Gameserver.Gameserver.GetModsBySteamToolId";
        public const string CreateMod = "PermissionConstants.Gameserver.Gameserver.CreateMod";
        public const string UpdateMod = "PermissionConstants.Gameserver.Gameserver.UpdateMod";
        public const string DeleteMod = "PermissionConstants.Gameserver.Gameserver.DeleteMod";
        public const string SearchMods = "PermissionConstants.Gameserver.Gameserver.SearchMods";
        public const string StartServer = "PermissionConstants.Gameserver.Gameserver.StartServer";
        public const string StopServer = "PermissionConstants.Gameserver.Gameserver.StopServer";
        public const string RestartServer = "PermissionConstants.Gameserver.Gameserver.RestartServer";
    }

    public static class Game
    {
        public const string GetAllPaginated = "PermissionConstants.Gameserver.Game.GetAllPaginated";
        public const string GetCount = "PermissionConstants.Gameserver.Game.GetCount";
        public const string Get = "PermissionConstants.Gameserver.Game.Get";
        public const string Create = "PermissionConstants.Gameserver.Game.Create";
        public const string Update = "PermissionConstants.Gameserver.Game.Update";
        public const string Delete = "PermissionConstants.Gameserver.Game.Delete";
        public const string Search = "PermissionConstants.Gameserver.Game.Search";
        public const string GetAllDevelopersPaginated = "PermissionConstants.Gameserver.Game.GetAllDevelopersPaginated";
        public const string GetDevelopersCount = "PermissionConstants.Gameserver.Game.GetDevelopersCount";
        public const string GetDeveloper = "PermissionConstants.Gameserver.Game.GetDeveloper";
        public const string GetDevelopers = "PermissionConstants.Gameserver.Game.GetDevelopers";
        public const string CreateDeveloper = "PermissionConstants.Gameserver.Game.CreateDeveloper";
        public const string DeleteDeveloper = "PermissionConstants.Gameserver.Game.DeleteDeveloper";
        public const string SearchDevelopers = "PermissionConstants.Gameserver.Game.SearchDevelopers";
        public const string GetAllPublishersPaginated = "PermissionConstants.Gameserver.Game.GetAllPublishersPaginated";
        public const string GetPublishersCount = "PermissionConstants.Gameserver.Game.GetPublishersCount";
        public const string GetPublisher = "PermissionConstants.Gameserver.Game.GetPublisher";
        public const string GetPublishers = "PermissionConstants.Gameserver.Game.GetPublishers";
        public const string CreatePublisher = "PermissionConstants.Gameserver.Game.CreatePublisher";
        public const string DeletePublisher = "PermissionConstants.Gameserver.Game.DeletePublisher";
        public const string SearchPublishers = "PermissionConstants.Gameserver.Game.SearchPublishers";
        public const string GetAllGameGenresPaginated = "PermissionConstants.Gameserver.Game.GetAllGameGenresPaginated";
        public const string GetGameGenresCount = "PermissionConstants.Gameserver.Game.GetGameGenresCount";
        public const string GetGameGenre = "PermissionConstants.Gameserver.Game.GetGameGenre";
        public const string GetGameGenres = "PermissionConstants.Gameserver.Game.GetGameGenres";
        public const string CreateGameGenre = "PermissionConstants.Gameserver.Game.CreateGameGenre";
        public const string DeleteGameGenre = "PermissionConstants.Gameserver.Game.DeleteGameGenre";
        public const string SearchGameGenres = "PermissionConstants.Gameserver.Game.SearchGameGenres";
    }

    public static class Network
    {
        public const string GameserverConnectable = "Permissions.GameServer.Network.GameserverConnectable";
        public const string IsPortOpen = "Permissions.GameServer.Network.IsPortOpen";
    }
}
