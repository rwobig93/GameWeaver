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
}
