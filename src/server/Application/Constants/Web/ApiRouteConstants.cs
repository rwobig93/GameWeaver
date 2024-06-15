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
            public const string GetToken = "/api/host/token";
            public const string GetAllPaginated = "/api/hosts";
            public const string GetById = "/api/host/id";
            public const string GetByHostname = "/api/host/hostname";
            public const string Create = "/api/host";
            public const string Update = "/api/host";
            public const string Delete = "/api/host";
            public const string Search = "/api/hosts/search";
        }

        public static class HostRegistration
        {
            public const string Create = "/api/host-registration/create";
            public const string Confirm = "/api/host-registration/confirm";
            public const string GetAll = "/api/host-registrations";
            public const string GetActive = "/api/host-registrations/active";
            public const string GetInActive = "/api/host-registrations/inactive";
            public const string GetCount = "/api/host-registrations/count";
            public const string Update = "/api/host-registration";
            public const string Search = "/api/host-registrations/search";
        }

        public static class HostCheckins
        {
            public const string CheckIn = "/api/host-checkin/checkin";
            public const string GetAll = "/api/host-checkins";
            public const string GetCount = "/api/host-checkins/count";
            public const string GetById = "/api/host-checkin/id";
            public const string GetByHost = "/api/host-checkins/host";
            public const string DeleteOld = "/api/host-checkins/old";
            public const string Search = "/api/host-checkins/search";
        }

        public static class WeaverWork
        {
            public const string GetAll = "/api/weaver/works";
            public const string GetCount = "/api/weaver/works/count";
            public const string GetById = "/api/weaver/work/id";
            public const string GetByStatus = "/api/weaver/works/status";
            public const string GetByType = "/api/weaver/works/type";
            public const string GetWaitingForHost = "/api/weaver/work/waiting";
            public const string GetAllWaitingForHost = "/api/weaver/works/waiting";
            public const string Create = "/api/weaver/work";
            public const string Update = "/api/weaver/work";
            public const string UpdateStatus = "/api/weaver/work/status";
            public const string Delete = "/api/weaver/work";
            public const string DeleteOld = "/api/weaver/works/old";
            public const string Search = "/api/weaver/works/search";
        }

        public static class Gameserver
        {
            public const string GetAll = "/api/gameservers";
            public const string GetCount = "/api/gameservers/count";
            public const string GetById = "/api/gameserver/id";
            public const string GetByServerName = "/api/gameserver/servername";
            public const string GetByGameId = "/api/gameservers/gameid";
            public const string GetByGameProfileId = "/api/gameservers/gameprofileid";
            public const string GetByHostId = "/api/gameservers/hostid";
            public const string GetByOwnerId = "/api/gameservers/ownerid";
            public const string Create = "/api/gameserver";
            public const string Update = "/api/gameserver";
            public const string Delete = "/api/gameserver";
            public const string Search = "/api/gameservers/search";
            public const string StartServer = "/api/gameserver/start";
            public const string StopServer = "/api/gameserver/stop";
            public const string RestartServer = "/api/gameserver/restart";
            public const string UpdateLocalResource = "/api/gameserver/update/local-resource";
            public const string UpdateAllLocalResources = "/api/gameserver/update/local-resources";
        }

        public static class ConfigItem
        {
            public const string GetAll = "/api/configitems";
            public const string GetCount = "/api/configitems/count";
            public const string GetById = "/api/configitem/id";
            public const string GetByLocalResource = "/api/configitems/localresource";
            public const string Create = "/api/configitem";
            public const string Update = "/api/configitem";
            public const string Delete = "/api/configitem";
            public const string Search = "/api/configitems/search";
        }

        public static class LocalResource
        {
            public const string GetAllPaginated = "/api/local-resources";
            public const string GetCount = "/api/local-resources/count";
            public const string GetById = "/api/local-resource/id";
            public const string GetByGameProfileId = "/api/local-resources/gameprofileid";
            public const string GetForGameServerId = "/api/local-resources/gameserverid";
            public const string Create = "/api/local-resource";
            public const string Update = "/api/local-resource";
            public const string Delete = "/api/local-resource";
            public const string Search = "/api/local-resources/search";
        }

        public static class GameProfile
        {
            public const string GetAll = "/api/game-profiles";
            public const string GetCount = "/api/game-profiles/count";
            public const string GetById = "/api/game-profile/id";
            public const string GetByFriendlyName = "/api/game-profile/friendlyname";
            public const string GetByGameId = "/api/game-profiles/gameid";
            public const string GetByOwnerId = "/api/game-profiles/ownerid";
            public const string Create = "/api/game-profile";
            public const string Update = "/api/game-profile";
            public const string Delete = "/api/game-profile";
            public const string Search = "/api/game-profiles/search";
        }

        public static class Mod
        {
            public const string GetAll = "/api/mods";
            public const string GetCount = "/api/mod/count";
            public const string GetById = "/api/mod/id";
            public const string GetByHash = "/api/mod/hash";
            public const string GetByFriendlyName = "/api/mods/friendlyname";
            public const string GetByGameId = "/api/mods/gameid";
            public const string GetBySteamGameId = "/api/mods/steamgameid";
            public const string GetBySteamId = "/api/mod/steamid";
            public const string GetBySteamToolId = "/api/mods/steamtoolid";
            public const string Create = "/api/mod";
            public const string Update = "/api/mod";
            public const string Delete = "/api/mod";
            public const string Search = "/api/mods/search";
        }

        public static class Game
        {
            public const string GetAllPaginated = "/api/gameserver/games";
            public const string GetCount = "/api/gameserver/game/count";
            public const string GetById = "/api/gameserver/game/id";
            public const string GetBySteamName = "/api/gameserver/game/steamname";
            public const string GetByFriendlyName = "/api/gameserver/game/friendlyname";
            public const string GetBySteamGameId = "/api/gameserver/game/steamgameid";
            public const string GetBySteamToolId = "/api/gameserver/game/steamtoolid";
            public const string Create = "/api/gameserver/game";
            public const string Update = "/api/gameserver/game";
            public const string Delete = "/api/gameserver/game";
            public const string Search = "/api/gameserver/game/search";
        }

        public static class GameGenre
        {
            public const string GetAll = "/api/game-genres";
            public const string GetCount = "/api/game-genre/count";
            public const string GetById = "/api/game-genre/id";
            public const string GetByName = "/api/game-genre/name";
            public const string GetByGameId = "/api/game-genres/gameid";
            public const string Create = "/api/game-genre";
            public const string Delete = "/api/game-genre";
            public const string Search = "/api/game-genres/search";
        }

        public static class Developer
        {
            public const string GetAll = "/api/developers";
            public const string GetCount = "/api/developer/count";
            public const string GetById = "/api/developer/id";
            public const string GetByName = "/api/developer/name";
            public const string GetByGameId = "/api/developers/gameid";
            public const string Create = "/api/developer";
            public const string Delete = "/api/developer";
            public const string Search = "/api/developers/search";
        }

        public static class Publisher
        {
            public const string GetAll = "/api/publishers";
            public const string GetCount = "/api/publisher/count";
            public const string GetById = "/api/publisher/id";
            public const string GetByName = "/api/publisher/name";
            public const string GetByGameId = "/api/publishers/gameid";
            public const string Create = "/api/publisher";
            public const string Delete = "/api/publisher";
            public const string Search = "/api/publishers/search";
        }

        public static class Network
        {
            public const string GameserverConnectable = "/api/network/gameserver/connectable";
            public const string IsPortOpen = "/api/network/port/open";
        }
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