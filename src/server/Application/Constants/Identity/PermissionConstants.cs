namespace Application.Constants.Identity;

public static class PermissionConstants
{
    // Built-in Permissions are the following format: Permissions.Group.Name.Access => Permissions.Identity.Users.View
    // Dynamic Permissions are the following format: Dynamic.Group.Id.AccessLevel => Dynamic.ServiceAccounts.<Guid>.Admin

    public static class Identity
    {
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
            public const string ViewExtAttrs = "Permissions.Identity.Users.ViewExtAttrs";
            public const string ChangeEmail = "Permissions.Identity.Users.ChangeEmail";
            public const string AdminEmail = "Permissions.Identity.Users.AdminEmail";
            public const string ForceLogin = "Permissions.Identity.Users.ForceLogin";
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
    }

    public static class System
    {
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

        public static class AppDevelopment
        {
            public const string Dev = "Permissions.System.Developer.Dev";
            public const string Contributor = "Permissions.System.Developer.Contributor";
            public const string Tester = "Permissions.System.Developer.Tester";
        }
    }

    public static class GameServer
    {
        public static class Hosts
        {
            public const string GetAll = "Permissions.GameServer.Host.GetAll";
            public const string GetAllPaginated = "Permissions.GameServer.Host.GetAllPaginated";
            public const string Get = "Permissions.GameServer.Host.Get";
            public const string Create = "Permissions.GameServer.Host.Create";
            public const string Update = "Permissions.GameServer.Host.Update";
            public const string Delete = "Permissions.GameServer.Host.Delete";
            public const string Search = "Permissions.GameServer.Host.Search";
            public const string SearchPaginated = "Permissions.GameServer.Host.SearchPaginated";
        }

        public static class HostRegistration
        {
            public const string Create = "Permissions.GameServer.HostRegistration.Create";
            public const string GetAll = "Permissions.GameServer.HostRegistration.GetAll";
            public const string GetAllPaginated = "Permissions.GameServer.HostRegistration.GetAllPaginated";
            public const string GetAllActive = "Permissions.GameServer.HostRegistration.GetAllActive";
            public const string GetAllInActive = "Permissions.GameServer.HostRegistration.GetAllInActive";
            public const string GetCount = "Permissions.GameServer.HostRegistration.GetCount";
            public const string Update = "Permissions.GameServer.HostRegistration.Update";
            public const string Search = "Permissions.GameServer.HostRegistration.Search";
            public const string SearchPaginated = "Permissions.GameServer.HostRegistration.SearchPaginated";
        }

        public static class HostCheckins
        {
            public const string CheckIn = "Permissions.GameServer.HostCheckin.CheckIn";
            public const string GetAll = "Permissions.GameServer.HostCheckin.GetAll";
            public const string GetAllPaginated = "Permissions.GameServer.HostCheckin.GetAllPaginated";
            public const string Get = "Permissions.GameServer.HostCheckin.Get";
            public const string GetCount = "Permissions.GameServer.HostCheckin.GetCount";
            public const string GetByHost = "Permissions.GameServer.HostCheckin.GetByHost";
            public const string DeleteOld = "Permissions.GameServer.HostCheckin.DeleteOld";
            public const string Search = "Permissions.GameServer.HostCheckin.Search";
            public const string SearchPaginated = "Permissions.GameServer.HostCheckin.SearchPaginated";
        }

        public static class WeaverWork
        {
            public const string GetAllPaginated = "Permissions.GameServer.WeaverWork.GetAllPaginated";
            public const string Get = "Permissions.GameServer.WeaverWork.Get";
            public const string GetCount = "Permissions.GameServer.WeaverWork.GetCount";
            public const string Create = "Permissions.GameServer.WeaverWork.Create";
            public const string Update = "Permissions.GameServer.WeaverWork.Update";
            public const string UpdateStatus = "Permissions.GameServer.WeaverWork.UpdateStatus";
            public const string Delete = "Permissions.GameServer.WeaverWork.Delete";
            public const string Search = "Permissions.GameServer.WeaverWork.Search";
            public const string SearchPaginated = "Permissions.GameServer.WeaverWork.SearchPaginated";
        }

        public static class Gameserver
        {
            public const string GetAllPaginated = "Permissions.GameServer.Gameserver.GetAllPaginated";
            public const string GetCount = "Permissions.GameServer.Gameserver.GetCount";
            public const string Get = "Permissions.GameServer.Gameserver.Get";
            public const string Create = "Permissions.GameServer.Gameserver.Create";
            public const string Update = "Permissions.GameServer.Gameserver.Update";
            public const string Delete = "Permissions.GameServer.Gameserver.Delete";
            public const string Search = "Permissions.GameServer.Gameserver.Search";
            public const string StartServer = "Permissions.GameServer.Gameserver.StartServer";
            public const string StopServer = "Permissions.GameServer.Gameserver.StopServer";
            public const string RestartServer = "Permissions.GameServer.Gameserver.RestartServer";
            public const string UpdateLocalResource = "Permissions.GameServer.Gameserver.UpdateLocalResource";
            public const string UpdateAllLocalResources = "Permissions.GameServer.Gameserver.UpdateAllLocalResources";
        }

        public static class ConfigItem
        {
            public const string GetAllPaginated = "Permissions.GameServer.ConfigItem.GetAllPaginated";
            public const string GetCount = "Permissions.GameServer.ConfigItem.GetCount";
            public const string Get = "Permissions.GameServer.ConfigItem.Get";
            public const string Create = "Permissions.GameServer.ConfigItem.Create";
            public const string Update = "Permissions.GameServer.ConfigItem.Update";
            public const string Delete = "Permissions.GameServer.ConfigItem.Delete";
            public const string Search = "Permissions.GameServer.ConfigItem.Search";
        }

        public static class LocalResource
        {
            public const string GetAllPaginated = "Permissions.GameServer.LocalResource.GetAllPaginated";
            public const string GetCount = "Permissions.GameServer.LocalResource.GetCount";
            public const string Get = "Permissions.GameServer.LocalResource.Get";
            public const string GetForGameServerId = "Permissions.GameServer.LocalResource.GetForGameServerId";
            public const string Create = "Permissions.GameServer.LocalResource.Create";
            public const string Update = "Permissions.GameServer.LocalResource.Update";
            public const string Delete = "Permissions.GameServer.LocalResource.Delete";
            public const string Search = "Permissions.GameServer.LocalResource.Search";
        }

        public static class GameProfile
        {
            public const string GetAllPaginated = "Permissions.GameServer.GameProfile.GetAllPaginated";
            public const string GetCount = "Permissions.GameServer.GameProfile.GetCount";
            public const string Get = "Permissions.GameServer.GameProfile.Get";
            public const string Create = "Permissions.GameServer.GameProfile.Create";
            public const string Update = "Permissions.GameServer.GameProfile.Update";
            public const string Delete = "Permissions.GameServer.GameProfile.Delete";
            public const string Search = "Permissions.GameServer.GameProfile.Search";
        }

        public static class Mod
        {
            public const string GetAllPaginated = "Permissions.GameServer.Mod.GetAllPaginated";
            public const string GetCount = "Permissions.GameServer.Mod.GetCount";
            public const string Get = "Permissions.GameServer.Mod.Get";
            public const string Create = "Permissions.GameServer.Mod.Create";
            public const string Update = "Permissions.GameServer.Mod.Update";
            public const string Delete = "Permissions.GameServer.Mod.Delete";
            public const string Search = "Permissions.GameServer.Mod.Search";
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
        }

        public static class GameGenre
        {
            public const string GetAllPaginated = "Permissions.GameServer.Publisher.GetAllPaginated";
            public const string GetCount = "Permissions.GameServer.GameGenre.GetCount";
            public const string Get = "Permissions.GameServer.GameGenre.Get";
            public const string Create = "Permissions.GameServer.GameGenre.Create";
            public const string Delete = "Permissions.GameServer.GameGenre.Delete";
            public const string Search = "Permissions.GameServer.GameGenre.Search";
        }

        public static class Developer
        {
            public const string GetAllPaginated = "Permissions.GameServer.Developer.GetAllPaginated";
            public const string GetCount = "Permissions.GameServer.Developer.GetCount";
            public const string Get = "Permissions.GameServer.Developer.Get";
            public const string Create = "Permissions.GameServer.Developer.Create";
            public const string Delete = "Permissions.GameServer.Developer.Delete";
            public const string Search = "Permissions.GameServer.Developer.Search";
        }

        public static class Publisher
        {
            public const string GetAllPaginated = "Permissions.GameServer.Publisher.GetAllPaginated";
            public const string GetCount = "Permissions.GameServer.Publisher.GetCount";
            public const string Get = "Permissions.GameServer.Publisher.Get";
            public const string Create = "Permissions.GameServer.Publisher.Create";
            public const string Delete = "Permissions.GameServer.Publisher.Delete";
            public const string Search = "Permissions.GameServer.Publisher.Search";
        }

        public static class Network
        {
            public const string GameserverConnectable = "Permissions.GameServer.Network.GameserverConnectable";
            public const string IsPortOpen = "Permissions.GameServer.Network.IsPortOpen";
        }
    }
}
