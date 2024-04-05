namespace Application.Constants.Web;

public static class ApiRouteConstants
{
    public static class Api
    {
        public const string GetToken = "/api/token";
        public const string WhoAmI = "/api/whoami";
    }

    public static class Monitoring
    {
        public const string Health = "/_health";
    }
    
    public static class Identity
    {
        public static class User
        {
            public const string GetAll = "/api/identity/users";
            public const string GetById = "/api/identity/user/id";
            public const string GetFullById = "/api/identity/user/id/full";
            public const string GetByEmail = "/api/identity/user/email";
            public const string GetFullByEmail = "/api/identity/user/email/full";
            public const string GetByUsername = "/api/identity/user/username";
            public const string GetFullByUsername = "/api/identity/user/username/full";
            public const string Delete = "/api/identity/user";
            public const string Create = "/api/identity/user";
            public const string Register = "/api/identity/user/register";
            public const string Login = "/api/identity/user/login";
            public const string Update = "/api/identity/user";
            public const string ResetPassword = "/api/identity/reset-password";
            public const string Enable = "/api/identity/user/enable";
            public const string Disable = "/api/identity/user/disable";
        }

        public static class Role
        {
            public const string GetAll = "/api/identity/roles";
            public const string GetById = "/api/identity/role";
            public const string Delete = "/api/identity/role";
            public const string Create = "/api/identity/role";
            public const string Update = "/api/identity/role";
        
            public const string GetRolesForUser = "/api/identity/roles/user";
            public const string IsUserInRole = "/api/identity/role/user/has";
            public const string AddUserToRole = "/api/identity/role/user/add";
            public const string RemoveUserFromRole = "/api/identity/role/user/remove";
        }

        public static class Permission
        {
            public const string GetAll = "/api/identity/permissions";
            public const string GetById = "/api/identity/permission";
            public const string Delete = "/api/identity/permission";
            public const string Update = "/api/identity/permission";
        
            public const string GetDirectPermissionsForUser = "/api/identity/permissions/user/direct";
            public const string GetAllPermissionsForUser = "/api/identity/permissions/user/all";
            public const string AddPermissionToUser = "/api/identity/permission/user/add";
            public const string RemovePermissionFromUser = "/api/identity/permission/user/remove";
            public const string DoesUserHavePermission = "/api/identity/permission/user/has";
        
            public const string GetAllPermissionsForRole = "/api/identity/permission/role";
            public const string AddPermissionToRole = "/api/identity/permission/role/add";
            public const string RemovePermissionFromRole = "/api/identity/permission/role/remove";
            public const string DoesRoleHavePermission = "/api/identity/permission/role/has";
        }
    }

    public static class Lifecycle
    {
        public static class Audit
        {
            public const string GetAll = "/api/lifecycle/audittrails";
            public const string GetById = "/api/lifecycle/audittrail";
            public const string GetByChangedBy = "/api/lifecycle/audittrail/changedby";
            public const string GetByRecordId = "/api/lifecycle/audittrail/record";
            public const string Delete = "/api/lifecycle/audittrail";
        }
    }

    public static class GameServer
    {
        public static class Host
        {
            public const string CreateRegistration = "/api/gameserver/host/create-registration";
            public const string RegistrationConfirm = "/api/gameserver/host/registration-confirm";
            public const string GetToken = "/api/gameserver/host/get-token";
            public const string CheckIn = "/api/gameserver/host/checkin";
            public const string UpdateWorkStatus = "/api/gameserver/host/work";
            public const string GetAll = "/api/gameserver/host/get-all";
            public const string GetAllPaginated = "/api/gameserver/host/get-all-paginated";
            public const string GetById = "/api/gameserver/host/get-id";
            public const string GetByHostname = "/api/gameserver/host/get-hostname";
            public const string Create = "/api/gameserver/host/create";
            public const string Update = "/api/gameserver/host/update";
            public const string Delete = "/api/gameserver/host/delete";
            public const string Search = "/api/gameserver/host/search";
            public const string SearchPaginated = "/api/gameserver/host/search-paginated";
            public const string GetAllRegistrations = "/api/gameserver/host/get-all-registrations";
            public const string GetAllRegistrationsPaginated = "/api/gameserver/host/get-all-registrations-paginated";
            public const string GetAllRegistrationsActive = "/api/gameserver/host/get-all-registrations-active";
            public const string GetAllRegistrationsInActive = "/api/gameserver/host/get-all-registrations-inactive";
            public const string GetRegistrationsCount = "/api/gameserver/host/get-registrations-count";
            public const string UpdateRegistration = "/api/gameserver/host/update-registration";
            public const string SearchRegistrations = "/api/gameserver/host/search-registrations";
            public const string SearchRegistrationsPaginated = "/api/gameserver/host/search-registrations-paginated";
            public const string GetAllCheckins = "/api/gameserver/host/get-all-checkins";
            public const string GetAllCheckinsPaginated = "/api/gameserver/host/get-all-checkins-paginated";
            public const string GetCheckinCount = "/api/gameserver/host/get-checkin-count";
            public const string GetCheckinById = "/api/gameserver/host/get-checkin-id";
            public const string GetCheckinByHost = "/api/gameserver/host/get-checkins-host";
            public const string DeleteOldCheckins = "/api/gameserver/host/delete-old-checkins";
            public const string SearchCheckins = "/api/gameserver/host/search-checkins";
            public const string SearchCheckinsPaginated = "/api/gameserver/host/search-checkins-paginated";
            public const string GetAllWeaverWorkPaginated = "/api/gameserver/host/get-all-weaverwork-paginated";
            public const string GetWeaverWorkCount = "/api/gameserver/host/get-weaverwork-count";
            public const string GetWeaverWorkById = "/api/gameserver/host/get-weaverwork-id";
            public const string GetWeaverWorkByStatus = "/api/gameserver/host/get-weaverwork-status";
            public const string GetWeaverWorkByType = "/api/gameserver/host/get-weaverwork-type";
            public const string GetWaitingWeaverWorkForHost = "/api/gameserver/host/get-weaverwork-waiting";
            public const string GetAllWaitingWeaverWorkForHost = "/api/gameserver/host/get-weaverwork-waiting-all";
            public const string CreateWeaverWork = "/api/gameserver/host/create-weaverwork";
            public const string UpdateWeaverWork = "/api/gameserver/host/update-weaverwork";
            public const string DeleteWeaverWork = "/api/gameserver/host/delete-weaverwork";
            public const string DeleteOldWeaverWork = "/api/gameserver/host/delete-old-weaverwork";
            public const string SearchWeaverWork = "/api/gameserver/host/search-weaverwork";
            public const string SearchWeaverWorkPaginated = "/api/gameserver/host/search-weaverwork-paginated";
        }

        public static class Gameserver
        {
            public const string GetAllPaginated = "/api/gameserver/Get-All-Paginated";
            public const string GetCount = "/api/gameserver/Get-Count";
            public const string GetById = "/api/gameserver/Get-Id";
            public const string GetByServerName = "/api/gameserver/Get-ServerName";
            public const string GetByGameId = "/api/gameserver/Get-Game-Id";
            public const string GetByGameProfileId = "/api/gameserver/Get-Game-GameProfile-Id";
            public const string GetByHostId = "/api/gameserver/Get-Host-Id";
            public const string GetByOwnerId = "/api/gameserver/Get-Owner-Id";
            public const string Create = "/api/gameserver/Create";
            public const string Update = "/api/gameserver/Update";
            public const string Delete = "/api/gameserver/Delete";
            public const string Search = "/api/gameserver/Search";
            public const string GetAllConfigurationItemsPaginated = "/api/gameserver/Get-All-ConfigurationItems-Paginated";
            public const string GetConfigurationItemsCount = "/api/gameserver/Get-ConfigurationItems-Count";
            public const string GetConfigurationItemById = "/api/gameserver/Get-ConfigurationItem-Id";
            public const string GetConfigurationItemsByGameProfileId = "/api/gameserver/Get-ConfigurationItems-GameProfile-Id";
            public const string CreateConfigurationItem = "/api/gameserver/Create-ConfigurationItem";
            public const string UpdateConfigurationItem = "/api/gameserver/Update-ConfigurationItem";
            public const string DeleteConfigurationItem = "/api/gameserver/Delete-ConfigurationItem";
            public const string SearchConfigurationItems = "/api/gameserver/Search-ConfigurationItems";
            public const string GetAllLocalResourcesPaginated = "/api/gameserver/Get-All-LocalResources-Paginated";
            public const string GetLocalResourcesCount = "/api/gameserver/Get-LocalResources-Count";
            public const string GetLocalResourceById = "/api/gameserver/Get-LocalResource-Id";
            public const string GetLocalResourcesByGameProfileId = "/api/gameserver/Get-LocalResources-GameProfile-Id";
            public const string GetLocalResourcesByGameServerId = "/api/gameserver/Get-LocalResources-GameServer-Id";
            public const string CreateLocalResource = "/api/gameserver/Create-LocalResource";
            public const string UpdateLocalResource = "/api/gameserver/Update-LocalResource";
            public const string DeleteLocalResource = "/api/gameserver/Delete-LocalResource";
            public const string SearchLocalResource = "/api/gameserver/Search-LocalResource";
            public const string GetAllGameProfilesPaginated = "/api/gameserver/Get-All-GameProfiles-Paginated";
            public const string GetGameProfileCount = "/api/gameserver/Get-GameProfile-Count";
            public const string GetGameProfileById = "/api/gameserver/Get-GameProfile-Id";
            public const string GetGameProfileByFriendlyName = "/api/gameserver/Get-GameProfile-FriendlyName";
            public const string GetGameProfilesByGameId = "/api/gameserver/Get-GameProfiles-GameId";
            public const string GetGameProfilesByOwnerId = "/api/gameserver/Get-GameProfiles-OwnerId";
            public const string GetGameProfilesByServerProcessName = "/api/gameserver/Get-GameProfiles-ServerProcessName";
            public const string CreateGameProfile = "/api/gameserver/Create-GameProfile";
            public const string UpdateGameProfile = "/api/gameserver/Update-GameProfile";
            public const string DeleteGameProfile = "/api/gameserver/Delete-GameProfile";
            public const string SearchGameProfiles = "/api/gameserver/Search-GameProfiles";
            public const string GetAllModsPaginated = "/api/gameserver/Get-All-Mods-Paginated";
            public const string GetModCount = "/api/gameserver/Get-Mod-Count";
            public const string GetModById = "/api/gameserver/Get-Mod-Id";
            public const string GetModByCurrentHash = "/api/gameserver/Get-Mod-CurrentHash";
            public const string GetModsByFriendlyName = "/api/gameserver/Get-Mods-FriendlyName";
            public const string GetModsByGameId = "/api/gameserver/Get-Mods-GameId";
            public const string GetModsBySteamGameId = "/api/gameserver/Get-Mods-SteamGameId";
            public const string GetModBySteamId = "/api/gameserver/Get-Mod-SteamId";
            public const string GetModsBySteamToolId = "/api/gameserver/Get-Mods-SteamToolId";
            public const string CreateMod = "/api/gameserver/Create-Mod";
            public const string UpdateMod = "/api/gameserver/Update-Mod";
            public const string DeleteMod = "/api/gameserver/Delete-Mod";
            public const string SearchMods = "/api/gameserver/Search-Mods";
        }

        public static class Game
        {
            public const string GetAll = "/api/gameserver/game/get-all";
            public const string GetAllPaginated = "/api/gameserver/game/get-all-paginated";
            public const string GetCount = "/api/gameserver/game/get-count";
            public const string GetById = "/api/gameserver/game/get-id";
            public const string GetBySteamName = "/api/gameserver/game/get-steamname";
            public const string GetByFriendlyName = "/api/gameserver/game/get-friendlyname";
            public const string GetBySteamGameId = "/api/gameserver/game/get-steamgameid";
            public const string GetBySteamToolId = "/api/gameserver/game/get-steamtoolid";
            public const string Create = "/api/gameserver/game/create";
            public const string Update = "/api/gameserver/game/update";
            public const string Delete = "/api/gameserver/game/delete";
            public const string Search = "/api/gameserver/game/search";
            public const string GetAllDevelopers = "/api/gameserver/game/get-all-developers";
            public const string GetAllDevelopersPaginated = "/api/gameserver/game/get-all-developers-paginated";
            public const string GetDevelopersCount = "/api/gameserver/game/get-developers-count";
            public const string GetDeveloperById = "/api/gameserver/game/get-developer-id";
            public const string GetDeveloperByName = "/api/gameserver/game/get-developer-name";
            public const string GetDevelopersByGameId = "/api/gameserver/game/get-developers-gameid";
            public const string CreateDeveloper = "/api/gameserver/game/create-developer";
            public const string DeleteDeveloper = "/api/gameserver/game/delete-developer";
            public const string SearchDevelopers = "/api/gameserver/game/search-developers";
            public const string GetAllPublishers = "/api/gameserver/game/get-all-publishers";
            public const string GetAllPublishersPaginated = "/api/gameserver/game/get-all-publishers-paginated";
            public const string GetPublishersCount = "/api/gameserver/game/get-publishers-count";
            public const string GetPublisherById = "/api/gameserver/game/get-publisher-id";
            public const string GetPublisherByName = "/api/gameserver/game/get-publisher-name";
            public const string GetPublishersByGameId = "/api/gameserver/game/get-publishers-gameid";
            public const string CreatePublisher = "/api/gameserver/game/create-publisher";
            public const string DeletePublisher = "/api/gameserver/game/delete-publisher";
            public const string SearchPublishers = "/api/gameserver/game/search-publishers";
            public const string GetAllGameGenres = "/api/gameserver/game/get-all-gamegenres";
            public const string GetAllGameGenresPaginated = "/api/gameserver/game/get-all-gamegenres-paginated";
            public const string GetGameGenresCount = "/api/gameserver/game/get-gamegenres-count";
            public const string GetGameGenreById = "/api/gameserver/game/get-gamegenre-id";
            public const string GetGameGenreByName = "/api/gameserver/game/get-gamegenre-name";
            public const string GetGameGenresByGameId = "/api/gameserver/game/get-gamegenres-gameid";
            public const string CreateGameGenre = "/api/gameserver/game/create-gamegenre";
            public const string DeleteGameGenre = "/api/gameserver/game/delete-gamegenre";
            public const string SearchGameGenres = "/api/gameserver/game/search-gamegenres";
        }

        public static class Network
        {
            public const string GameserverConnectable = "/api/network/gameserver-connectable";
            public const string IsPortOpen = "/api/network/is-port-open";
        }
    }

    public static class Example
    {
        public const string Weather = "/api/example/weather";
    }
}

public static class ApiRouteExtensions
{
    public static string ToFullUrl(this string uri, string hostOrigin)
    {
        if (hostOrigin.EndsWith('/'))
            hostOrigin = hostOrigin[..^1];
        
        return string.Concat(hostOrigin, uri);
    }
}