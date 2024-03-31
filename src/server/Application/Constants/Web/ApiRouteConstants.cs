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
            public const string GetCheckin = "/api/gameserver/host/get-checkin";
            public const string GetCheckinByHost = "/api/gameserver/host/get-checkins-host";
            public const string DeleteOldCheckins = "/api/gameserver/host/delete-old-checkins";
            public const string SearchCheckins = "/api/gameserver/host/search-checkins";
            public const string SearchCheckinsPaginated = "/api/gameserver/host/search-checkins-paginated";
            public const string GetAllWeaverWorkPaginated = "/api/gameserver/host/get-all-weaverwork-paginated";
            public const string GetWeaverWorkCount = "/api/gameserver/host/get-weaverwork-count";
            public const string GetWeaverWork = "/api/gameserver/host/get-weaverwork";
            public const string CreateWeaverWork = "/api/gameserver/host/create-weaverwork";
            public const string UpdateWeaverWork = "/api/gameserver/host/update-weaverwork";
            public const string DeleteWeaverWork = "/api/gameserver/host/delete-weaverwork";
            public const string SearchWeaverWork = "/api/gameserver/host/search-weaverwork";
            public const string SearchWeaverWorkPaginated = "/api/gameserver/host/search-weaverwork-paginated";
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