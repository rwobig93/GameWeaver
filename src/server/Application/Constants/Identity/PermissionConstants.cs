using Domain.Enums.Identity;

namespace Application.Constants.Identity;

public static class PermissionConstants
{
    public static class Identity
    {
        public static class Users
        {
            public const string View = $"{ClaimConstants.Permission}.Identity.Users.View";
            public const string Create = $"{ClaimConstants.Permission}.Identity.Users.Create";
            public const string Edit = $"{ClaimConstants.Permission}.Identity.Users.Edit";
            public const string Delete = $"{ClaimConstants.Permission}.Identity.Users.Delete";
            public const string Export = $"{ClaimConstants.Permission}.Identity.Users.Export";
            public const string Enable = $"{ClaimConstants.Permission}.Identity.Users.Enable";
            public const string Disable = $"{ClaimConstants.Permission}.Identity.Users.Disable";
            public const string ResetPassword = $"{ClaimConstants.Permission}.Identity.Users.ResetPassword";
            public const string ViewExtAttrs = $"{ClaimConstants.Permission}.Identity.Users.ViewExtAttrs";
            public const string ChangeEmail = $"{ClaimConstants.Permission}.Identity.Users.ChangeEmail";
            public const string AdminEmail = $"{ClaimConstants.Permission}.Identity.Users.AdminEmail";
            public const string ForceLogin = $"{ClaimConstants.Permission}.Identity.Users.ForceLogin";
        }

        public static class ServiceAccounts
        {
            public const string View = $"{ClaimConstants.Permission}.Identity.ServiceAccounts.View";
            public const string Admin = $"{ClaimConstants.Permission}.Identity.ServiceAccounts.Admin";

            public static string Dynamic(Guid id, DynamicPermissionLevel permission) => $"{ClaimConstants.DynamicPermission}.{DynamicPermissionGroup.ServiceAccounts}.{id}.{permission}";
        }

        public static class Roles
        {
            public const string View = $"{ClaimConstants.Permission}.Identity.Roles.View";
            public const string Create = $"{ClaimConstants.Permission}.Identity.Roles.Create";
            public const string Edit = $"{ClaimConstants.Permission}.Identity.Roles.Edit";
            public const string Delete = $"{ClaimConstants.Permission}.Identity.Roles.Delete";
            public const string Add = $"{ClaimConstants.Permission}.Identity.Roles.Add";
            public const string Remove = $"{ClaimConstants.Permission}.Identity.Roles.Remove";
            public const string Export = $"{ClaimConstants.Permission}.Identity.Roles.Export";
        }

        public static class Permissions
        {
            public const string View = $"{ClaimConstants.Permission}.Identity.Permissions.View";
            public const string Add = $"{ClaimConstants.Permission}.Identity.Permissions.Add";
            public const string Remove = $"{ClaimConstants.Permission}.Identity.Permissions.Remove";
        }

        public static class Preferences
        {
            public const string ChangeTheme = $"{ClaimConstants.Permission}.Identity.Preferences.ChangeTheme";
        }
    }

    public static class System
    {
        public static class Jobs
        {
            public const string View = $"{ClaimConstants.Permission}.System.Jobs.View";
            public const string Status = $"{ClaimConstants.Permission}.System.Jobs.Status";
        }

        public static class Api
        {
            public const string View = $"{ClaimConstants.Permission}.System.Api.View";
            public const string GenerateToken = $"{ClaimConstants.Permission}.System.Api.GenerateToken";
        }

        public static class Audit
        {
            public const string View = $"{ClaimConstants.Permission}.System.Audit.View";
            public const string Export = $"{ClaimConstants.Permission}.System.Audit.Export";
            public const string Search = $"{ClaimConstants.Permission}.System.Audit.Search";
            public const string DeleteOld = $"{ClaimConstants.Permission}.System.Audit.DeleteOld";
        }

        public static class Troubleshooting
        {
            public const string View = $"{ClaimConstants.Permission}.System.Troubleshooting.View";
            public const string Export = $"{ClaimConstants.Permission}.System.Troubleshooting.Export";
            public const string Search = $"{ClaimConstants.Permission}.System.Troubleshooting.Search";
            public const string DeleteOld = $"{ClaimConstants.Permission}.System.Troubleshooting.DeleteOld";
        }

        public static class AppDevelopment
        {
            public const string Dev = $"{ClaimConstants.Permission}.System.Developer.Dev";
            public const string Contributor = $"{ClaimConstants.Permission}.System.Developer.Contributor";
            public const string Tester = $"{ClaimConstants.Permission}.System.Developer.Tester";
        }
    }

    public static class GameServer
    {
        public static class Hosts
        {
            public const string GetAll = $"{ClaimConstants.Permission}.GameServer.Host.GetAll";
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.Host.GetAllPaginated";
            public const string SeeUi = $"{ClaimConstants.Permission}.GameServer.Host.SeeUi";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.Host.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.Host.Create";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.Host.Update";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.Host.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.Host.Search";
            public const string ChangeOwnership = $"{ClaimConstants.Permission}.GameServer.Host.ChangeOwnership";
            public const string SearchPaginated = $"{ClaimConstants.Permission}.GameServer.Host.SearchPaginated";
        }

        public static class HostRegistration
        {
            public const string Create = $"{ClaimConstants.Permission}.GameServer.HostRegistration.Create";
            public const string GetAll = $"{ClaimConstants.Permission}.GameServer.HostRegistration.GetAll";
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.HostRegistration.GetAllPaginated";
            public const string GetAllActive = $"{ClaimConstants.Permission}.GameServer.HostRegistration.GetAllActive";
            public const string GetAllInActive = $"{ClaimConstants.Permission}.GameServer.HostRegistration.GetAllInActive";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.HostRegistration.GetCount";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.HostRegistration.Update";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.HostRegistration.Search";
            public const string SearchPaginated = $"{ClaimConstants.Permission}.GameServer.HostRegistration.SearchPaginated";
        }

        public static class HostCheckins
        {
            public const string CheckIn = $"{ClaimConstants.Permission}.GameServer.HostCheckin.CheckIn";
            public const string GetAll = $"{ClaimConstants.Permission}.GameServer.HostCheckin.GetAll";
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.HostCheckin.GetAllPaginated";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.HostCheckin.Get";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.HostCheckin.GetCount";
            public const string GetByHost = $"{ClaimConstants.Permission}.GameServer.HostCheckin.GetByHost";
            public const string DeleteOld = $"{ClaimConstants.Permission}.GameServer.HostCheckin.DeleteOld";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.HostCheckin.Search";
            public const string SearchPaginated = $"{ClaimConstants.Permission}.GameServer.HostCheckin.SearchPaginated";
        }

        public static class WeaverWork
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.WeaverWork.GetAllPaginated";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.WeaverWork.Get";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.WeaverWork.GetCount";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.WeaverWork.Create";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.WeaverWork.Update";
            public const string UpdateStatus = $"{ClaimConstants.Permission}.GameServer.WeaverWork.UpdateStatus";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.WeaverWork.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.WeaverWork.Search";
            public const string SearchPaginated = $"{ClaimConstants.Permission}.GameServer.WeaverWork.SearchPaginated";
        }

        public static class Gameserver
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.Gameservers.GetAllPaginated";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.Gameservers.GetCount";
            public const string SeeUi = $"{ClaimConstants.Permission}.GameServer.Gameserver.SeeUi";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.Gameservers.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.Gameservers.Create";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.Gameservers.Update";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.Gameservers.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.Gameservers.Search";
            public const string ChangeOwnership = $"{ClaimConstants.Permission}.GameServer.Gameservers.ChangeOwnership";
            public const string StartServer = $"{ClaimConstants.Permission}.GameServer.Gameservers.StartServer";
            public const string StopServer = $"{ClaimConstants.Permission}.GameServer.Gameservers.StopServer";
            public const string RestartServer = $"{ClaimConstants.Permission}.GameServer.Gameservers.RestartServer";
            public const string UpdateLocalResource = $"{ClaimConstants.Permission}.GameServer.Gameservers.UpdateLocalResource";
            public const string UpdateAllLocalResources = $"{ClaimConstants.Permission}.GameServer.Gameservers.UpdateAllLocalResources";

            public static string Dynamic(Guid id, DynamicPermissionLevel permission) => $"{ClaimConstants.DynamicPermission}.{DynamicPermissionGroup.GameServers}.{id}.{permission}";
        }

        public static class ConfigItem
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.ConfigItem.GetAllPaginated";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.ConfigItem.GetCount";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.ConfigItem.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.ConfigItem.Create";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.ConfigItem.Update";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.ConfigItem.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.ConfigItem.Search";
        }

        public static class LocalResource
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.LocalResource.GetAllPaginated";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.LocalResource.GetCount";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.LocalResource.Get";
            public const string GetForGameServerId = $"{ClaimConstants.Permission}.GameServer.LocalResource.GetForGameServerId";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.LocalResource.Create";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.LocalResource.Update";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.LocalResource.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.LocalResource.Search";
        }

        public static class GameProfile
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.GameProfile.GetAllPaginated";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.GameProfile.GetCount";
            public const string SeeUi = $"{ClaimConstants.Permission}.GameServer.GameProfile.SeeUi";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.GameProfile.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.GameProfile.Create";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.GameProfile.Update";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.GameProfile.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.GameProfile.Search";
            public const string ChangeOwnership = $"{ClaimConstants.Permission}.GameServer.Gameservers.ChangeOwnership";

            public static string Dynamic(Guid id, DynamicPermissionLevel permission) => $"{ClaimConstants.DynamicPermission}.{DynamicPermissionGroup.GameProfiles}.{id}.{permission}";
        }

        public static class Mod
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.Mod.GetAllPaginated";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.Mod.GetCount";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.Mod.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.Mod.Create";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.Mod.Update";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.Mod.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.Mod.Search";
        }

        public static class Game
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.Game.GetAllPaginated";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.Game.GetCount";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.Game.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.Game.Create";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.Game.Update";
            public const string Configure = $"{ClaimConstants.Permission}.GameServer.Game.Configure";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.Game.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.Game.Search";
            public const string DownloadLatest = $"{ClaimConstants.Permission}.GameServer.Game.Download";
        }

        public static class GameVersions
        {
            public const string Get = $"{ClaimConstants.Permission}.GameServer.Game.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.Game.Create";
            public const string Update = $"{ClaimConstants.Permission}.GameServer.Game.Update";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.Game.Delete";
        }

        public static class GameGenre
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.Publisher.GetAllPaginated";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.GameGenre.GetCount";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.GameGenre.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.GameGenre.Create";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.GameGenre.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.GameGenre.Search";
        }

        public static class Developer
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.Developer.GetAllPaginated";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.Developer.GetCount";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.Developer.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.Developer.Create";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.Developer.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.Developer.Search";
        }

        public static class Publisher
        {
            public const string GetAllPaginated = $"{ClaimConstants.Permission}.GameServer.Publisher.GetAllPaginated";
            public const string GetCount = $"{ClaimConstants.Permission}.GameServer.Publisher.GetCount";
            public const string Get = $"{ClaimConstants.Permission}.GameServer.Publisher.Get";
            public const string Create = $"{ClaimConstants.Permission}.GameServer.Publisher.Create";
            public const string Delete = $"{ClaimConstants.Permission}.GameServer.Publisher.Delete";
            public const string Search = $"{ClaimConstants.Permission}.GameServer.Publisher.Search";
        }

        public static class Network
        {
            public const string GameserverConnectable = $"{ClaimConstants.Permission}.GameServer.Network.GameserverConnectable";
            public const string IsPortOpen = $"{ClaimConstants.Permission}.GameServer.Network.IsPortOpen";
        }
    }
}
