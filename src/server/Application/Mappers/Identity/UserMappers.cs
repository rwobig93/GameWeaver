using Application.Models.Identity.User;
using Application.Models.Identity.UserExtensions;
using Application.Requests.Identity.User;
using Application.Responses.v1.Identity;
using Domain.DatabaseEntities.Identity;
using Domain.Enums.Identity;
using Domain.Models.Identity;

namespace Application.Mappers.Identity;

public static class UserMappers
{
    public static AppUserSlim ToSlim(this AppUserFull appUserFull)
    {
        return new AppUserSlim
        {
            Id = appUserFull.Id,
            Username = appUserFull.Username,
            EmailAddress = appUserFull.EmailAddress,
            FirstName = appUserFull.FirstName,
            LastName = appUserFull.LastName,
            CreatedBy = appUserFull.CreatedBy,
            ProfilePictureDataUrl = appUserFull.ProfilePictureDataUrl,
            CreatedOn = appUserFull.CreatedOn,
            LastModifiedBy = appUserFull.LastModifiedBy,
            LastModifiedOn = appUserFull.LastModifiedOn,
            IsDeleted = appUserFull.IsDeleted,
            DeletedOn = appUserFull.DeletedOn,
            AccountType = appUserFull.AccountType,
            Notes = appUserFull.Notes,
            Currency = appUserFull.Currency,
            AuthState = appUserFull.AuthState
        };
    }

    public static AppUserSlim ToSlim(this AppUserDb appUserDb)
    {
        return new AppUserSlim
        {
            Id = appUserDb.Id,
            Username = appUserDb.Username,
            EmailAddress = appUserDb.Email,
            FirstName = appUserDb.FirstName,
            LastName = appUserDb.LastName,
            CreatedBy = appUserDb.CreatedBy,
            ProfilePictureDataUrl = appUserDb.ProfilePictureDataUrl,
            CreatedOn = appUserDb.CreatedOn,
            LastModifiedBy = appUserDb.LastModifiedBy,
            LastModifiedOn = appUserDb.LastModifiedOn,
            IsDeleted = appUserDb.IsDeleted,
            DeletedOn = appUserDb.DeletedOn,
            AccountType = appUserDb.AccountType,
            Notes = appUserDb.Notes,
            Currency = appUserDb.Currency,
            AuthState = AuthState.Enabled
        };
    }

    public static IEnumerable<AppUserSlim> ToSlims(this IEnumerable<AppUserDb> appUserDbs)
    {
        return appUserDbs.Select(x => x.ToSlim());
    }

    public static UserBasicResponse ToResponse(this AppUserSlim appUser)
    {
        return new UserBasicResponse
        {
            Id = appUser.Id,
            Username = appUser.Username,
            CreatedOn = appUser.CreatedOn,
            AuthState = appUser.AuthState.ToString(),
            AccountType = appUser.AccountType.ToString()
        };
    }

    public static List<UserBasicResponse> ToResponses(this IEnumerable<AppUserSlim> appUsers)
    {
        return appUsers.Select(x => x.ToResponse()).ToList();
    }

    public static UserFullResponse ToFullResponse(this AppUserFull appUser)
    {
        return new UserFullResponse
        {
            Id = appUser.Id,
            Username = appUser.Username,
            CreatedOn = appUser.CreatedOn,
            AuthState = appUser.AuthState.ToString(),
            AccountType = appUser.AccountType.ToString(),
            Currency = appUser.Currency,
            ExtendedAttributes = appUser.ExtendedAttributes.ToResponses(),
            Permissions = appUser.Permissions.ToResponses()
        };
    }

    public static AppUserFull ToFull(this AppUserSlim appUser)
    {
        return new AppUserFull
        {
            Id = appUser.Id,
            Username = appUser.Username,
            EmailAddress = appUser.EmailAddress,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            CreatedBy = appUser.CreatedBy,
            ProfilePictureDataUrl = appUser.ProfilePictureDataUrl,
            CreatedOn = appUser.CreatedOn,
            LastModifiedBy = appUser.LastModifiedBy,
            LastModifiedOn = appUser.LastModifiedOn,
            IsDeleted = appUser.IsDeleted,
            DeletedOn = appUser.DeletedOn,
            AccountType = appUser.AccountType,
            Roles = [],
            ExtendedAttributes = [],
            Permissions = [],
            AuthState = appUser.AuthState,
            Notes = appUser.Notes,
            Currency = appUser.Currency
        };
    }

    public static AppUserFull ToFull(this AppUserFullDb appUser)
    {
        return new AppUserFull
        {
            Id = appUser.Id,
            Username = appUser.Username,
            EmailAddress = appUser.Email,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            CreatedBy = appUser.CreatedBy,
            ProfilePictureDataUrl = appUser.ProfilePictureDataUrl,
            CreatedOn = appUser.CreatedOn,
            LastModifiedBy = appUser.LastModifiedBy,
            LastModifiedOn = appUser.LastModifiedOn,
            IsDeleted = appUser.IsDeleted,
            DeletedOn = appUser.DeletedOn,
            AccountType = appUser.AccountType,
            Roles = [],
            ExtendedAttributes = [],
            Permissions = [],
            AuthState = appUser.AuthState,
            Notes = appUser.Notes,
            Currency = appUser.Currency
        };
    }

    public static UserFullResponse ToResponse(this AppUserFull appUser)
    {
        return new UserFullResponse
        {
            Id = appUser.Id,
            Username = appUser.Username,
            CreatedOn = appUser.CreatedOn,
            AuthState = appUser.AuthState.ToString(),
            AccountType = appUser.AccountType.ToString(),
            Currency = appUser.Currency,
            ExtendedAttributes = appUser.ExtendedAttributes.ToResponses(),
            Permissions = appUser.Permissions.ToResponses()
        };
    }

    public static List<UserFullResponse> ToResponses(this IEnumerable<AppUserFull> appUsers)
    {
        return appUsers.Select(x => x.ToResponse()).ToList();
    }

    public static AppUserCreate ToCreateObject(this AppUserDb appUser)
    {
        return new AppUserCreate
        {
            Username = appUser.Username,
            Email = appUser.Email,
            EmailConfirmed = appUser.EmailConfirmed,
            PhoneNumber = appUser.PhoneNumber,
            PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            ProfilePictureDataUrl = appUser.ProfilePictureDataUrl,
            CreatedBy = appUser.CreatedBy,
            CreatedOn = appUser.CreatedOn,
            LastModifiedBy = appUser.LastModifiedBy,
            LastModifiedOn = appUser.LastModifiedOn,
            IsDeleted = appUser.IsDeleted,
            DeletedOn = appUser.DeletedOn,
            AccountType = appUser.AccountType,
            Notes = appUser.Notes,
            Currency = appUser.Currency
        };
    }

    public static AppUserCreate ToCreateObject(this UserCreateRequest appUser)
    {
        return new AppUserCreate
        {
            Username = appUser.Username,
            Email = appUser.Email,
            EmailConfirmed = appUser.EmailConfirmed,
            PhoneNumber = appUser.PhoneNumber,
            PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            ProfilePictureDataUrl = "",
            CreatedBy = Guid.Empty,
            CreatedOn = DateTime.Now,
            LastModifiedBy = Guid.Empty,
            LastModifiedOn = DateTime.Now,
            IsDeleted = false,
            DeletedOn = null,
            AccountType = AccountType.User,
            Notes = null,
            Currency = 0
        };
    }

    public static AppUserExtendedAttributeSlim ToSlim(this AppUserExtendedAttributeDb extendedAttributeDb)
    {
        return new AppUserExtendedAttributeSlim
        {
            Id = extendedAttributeDb.Id,
            OwnerId = extendedAttributeDb.OwnerId,
            Name = extendedAttributeDb.Name,
            Value = extendedAttributeDb.Value,
            Description = extendedAttributeDb.Description,
            Type = extendedAttributeDb.Type
        };
    }

    public static IEnumerable<AppUserExtendedAttributeSlim> ToSlims(this IEnumerable<AppUserExtendedAttributeDb> extendedAttributeDbs)
    {
        return extendedAttributeDbs.Select(x => x.ToSlim());
    }

    public static ExtendedAttributeResponse ToResponse(this AppUserExtendedAttributeSlim attribute)
    {
        return new ExtendedAttributeResponse
        {
            Id = attribute.Id,
            OwnerId = attribute.OwnerId,
            Name = attribute.Name,
            Value = attribute.Value,
            Type = attribute.Type.ToString()
        };
    }

    public static List<ExtendedAttributeResponse> ToResponses(this IEnumerable<AppUserExtendedAttributeSlim> attributes)
    {
        return attributes.Select(x => x.ToResponse()).ToList();
    }

    public static AppUserUpdate ToUpdate(this AppUserDb appUser)
    {
        return new AppUserUpdate
        {
            Id = appUser.Id,
            Username = appUser.Username,
            Email = appUser.Email,
            EmailConfirmed = appUser.EmailConfirmed,
            PhoneNumber = appUser.PhoneNumber,
            PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            ProfilePictureDataUrl = appUser.ProfilePictureDataUrl,
            LastModifiedBy = appUser.LastModifiedBy,
            LastModifiedOn = appUser.LastModifiedOn,
            AccountType = appUser.AccountType,
            Notes = appUser.Notes,
            Currency = appUser.Currency
        };
    }

    public static AppUserUpdate ToUpdate(this UserUpdateRequest appUser)
    {
        return new AppUserUpdate
        {
            Id = appUser.Id,
            Username = null,
            Email = null,
            EmailConfirmed = null,
            PhoneNumber = null,
            PhoneNumberConfirmed = null,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            ProfilePictureDataUrl = appUser.ProfilePictureDataUrl,
            LastModifiedBy = null,
            LastModifiedOn = null,
            AccountType = AccountType.User
        };
    }

    public static AppUserUpdate ToUpdate(this AppUserFull appUser)
    {
        return new AppUserUpdate
        {
            Id = appUser.Id,
            Username = appUser.Username,
            Email = appUser.EmailAddress,
            EmailConfirmed = null,
            PhoneNumber = null,
            PhoneNumberConfirmed = null,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            ProfilePictureDataUrl = appUser.ProfilePictureDataUrl,
            LastModifiedBy = appUser.LastModifiedBy,
            LastModifiedOn = appUser.LastModifiedOn,
            AccountType = appUser.AccountType,
            Notes = appUser.Notes,
            Currency = appUser.Currency
        };
    }

    public static AppUserSecurityAttributeInfo ToInfo(this AppUserSecurityAttributeDb securityDb)
    {
        return new AppUserSecurityAttributeInfo
        {
            Id = securityDb.Id,
            OwnerId = securityDb.OwnerId,
            TwoFactorEnabled = securityDb.TwoFactorEnabled,
            AuthState = securityDb.AuthState,
            AuthStateTimestamp = securityDb.AuthStateTimestamp,
            BadPasswordAttempts = securityDb.BadPasswordAttempts,
            LastBadPassword = securityDb.LastBadPassword,
            LastFullLogin = securityDb.LastFullLogin
        };
    }

    public static AppUserSecurityAttributeUpdate ToUpdate(this AppUserSecurityAttributeDb securityDb)
    {
        return new AppUserSecurityAttributeUpdate
        {
            OwnerId = securityDb.OwnerId,
            PasswordHash = securityDb.PasswordHash,
            PasswordSalt = securityDb.PasswordSalt,
            TwoFactorEnabled = securityDb.TwoFactorEnabled,
            TwoFactorKey = securityDb.TwoFactorKey,
            AuthState = securityDb.AuthState,
            AuthStateTimestamp = securityDb.AuthStateTimestamp,
            BadPasswordAttempts = securityDb.BadPasswordAttempts,
            LastBadPassword = securityDb.LastBadPassword,
            LastFullLogin = securityDb.LastFullLogin
        };
    }

    public static AppUserSecurityFull ToUserSecurityFull(this AppUserSecurityDb securityDb)
    {
        return new AppUserSecurityFull
        {
            Id = securityDb.Id,
            Username = securityDb.Username,
            Email = securityDb.Email,
            EmailConfirmed = securityDb.EmailConfirmed,
            PhoneNumber = securityDb.PhoneNumber,
            PhoneNumberConfirmed = securityDb.PhoneNumberConfirmed,
            FirstName = securityDb.FirstName,
            LastName = securityDb.LastName,
            CreatedBy = securityDb.CreatedBy,
            ProfilePictureDataUrl = securityDb.ProfilePictureDataUrl,
            CreatedOn = securityDb.CreatedOn,
            LastModifiedBy = securityDb.LastModifiedBy,
            LastModifiedOn = securityDb.LastModifiedOn,
            IsDeleted = securityDb.IsDeleted,
            DeletedOn = securityDb.DeletedOn,
            AccountType = securityDb.AccountType,
            Notes = securityDb.Notes,
            Currency = securityDb.Currency,
            PasswordHash = securityDb.PasswordHash,
            PasswordSalt = securityDb.PasswordSalt,
            AuthState = securityDb.AuthState,
            AuthStateTimestamp = securityDb.AuthStateTimestamp,
            BadPasswordAttempts = securityDb.BadPasswordAttempts,
            LastBadPassword = securityDb.LastBadPassword,
            LastFullLogin = securityDb.LastFullLogin,
            TwoFactorEnabled = securityDb.TwoFactorEnabled,
            TwoFactorKey = securityDb.TwoFactorKey
        };
    }

    public static AppUserUpdate ToUserUpdate(this AppUserSecurityDb appUser)
    {
        return new AppUserUpdate
        {
            Id = appUser.Id,
            Username = appUser.Username,
            Email = appUser.Email,
            EmailConfirmed = appUser.EmailConfirmed,
            PhoneNumber = appUser.PhoneNumber,
            PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            ProfilePictureDataUrl = appUser.ProfilePictureDataUrl,
            LastModifiedBy = appUser.LastModifiedBy,
            LastModifiedOn = appUser.LastModifiedOn,
            AccountType = appUser.AccountType,
            Notes = appUser.Notes,
            Currency = appUser.Currency
        };
    }

    public static AppUserSecurityAttributeUpdate ToSecurityUpdate(this AppUserSecurityDb appUser)
    {
        return new AppUserSecurityAttributeUpdate
        {
            OwnerId = appUser.Id,
            PasswordHash = appUser.PasswordHash,
            PasswordSalt = appUser.PasswordSalt,
            TwoFactorEnabled = appUser.TwoFactorEnabled,
            TwoFactorKey = appUser.TwoFactorKey,
            AuthState = appUser.AuthState,
            AuthStateTimestamp = appUser.AuthStateTimestamp,
            BadPasswordAttempts = appUser.BadPasswordAttempts,
            LastBadPassword = appUser.LastBadPassword,
            LastFullLogin = appUser.LastFullLogin
        };
    }

    public static AppUserUpdate ToUserUpdate(this AppUserSecurityFull appUser)
    {
        return new AppUserUpdate
        {
            Id = appUser.Id,
            Username = appUser.Username,
            Email = appUser.Email,
            EmailConfirmed = appUser.EmailConfirmed,
            PhoneNumber = appUser.PhoneNumber,
            PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            ProfilePictureDataUrl = appUser.ProfilePictureDataUrl,
            LastModifiedBy = appUser.LastModifiedBy,
            LastModifiedOn = appUser.LastModifiedOn,
            AccountType = appUser.AccountType,
            Notes = appUser.Notes,
            Currency = appUser.Currency
        };
    }

    public static AppUserSecurityAttributeUpdate ToSecurityUpdate(this AppUserSecurityFull appUser)
    {
        return new AppUserSecurityAttributeUpdate
        {
            OwnerId = appUser.Id,
            PasswordHash = appUser.PasswordHash,
            PasswordSalt = appUser.PasswordSalt,
            TwoFactorEnabled = appUser.TwoFactorEnabled,
            TwoFactorKey = appUser.TwoFactorKey,
            AuthState = appUser.AuthState,
            AuthStateTimestamp = appUser.AuthStateTimestamp,
            BadPasswordAttempts = appUser.BadPasswordAttempts,
            LastBadPassword = appUser.LastBadPassword,
            LastFullLogin = appUser.LastFullLogin
        };
    }

    public static AppUserDb ToUserDb(this AppUserSecurityDb securityDb)
    {
        return new AppUserDb
        {
            Id = securityDb.Id,
            Username = securityDb.Username,
            Email = securityDb.Email,
            EmailConfirmed = securityDb.EmailConfirmed,
            PhoneNumber = securityDb.PhoneNumber,
            PhoneNumberConfirmed = securityDb.PhoneNumberConfirmed,
            FirstName = securityDb.FirstName,
            LastName = securityDb.LastName,
            CreatedBy = securityDb.CreatedBy,
            ProfilePictureDataUrl = securityDb.ProfilePictureDataUrl,
            CreatedOn = securityDb.CreatedOn,
            LastModifiedBy = securityDb.LastModifiedBy,
            LastModifiedOn = securityDb.LastModifiedOn,
            IsDeleted = securityDb.IsDeleted,
            DeletedOn = securityDb.DeletedOn,
            AccountType = securityDb.AccountType,
            Notes = securityDb.Notes,
            Currency = securityDb.Currency
        };
    }

    public static AppUserUpdate ToUpdate(this AppUserSecurityDb userDb)
    {
        return new AppUserUpdate
        {
            Id = userDb.Id,
            Username = userDb.Username,
            Email = userDb.Email,
            EmailConfirmed = userDb.EmailConfirmed,
            PhoneNumber = userDb.PhoneNumber,
            PhoneNumberConfirmed = userDb.PhoneNumberConfirmed,
            FirstName = userDb.FirstName,
            LastName = userDb.LastName,
            ProfilePictureDataUrl = userDb.ProfilePictureDataUrl,
            LastModifiedBy = userDb.LastModifiedBy,
            LastModifiedOn = userDb.LastModifiedOn,
            AccountType = userDb.AccountType,
            Notes = userDb.Notes,
            Currency = userDb.Currency
        };
    }

    public static AppUserSlim ToSlim(this AppUserSecurityDb userDb)
    {
        return new AppUserSlim
        {
            Id = userDb.Id,
            Username = userDb.Username,
            EmailAddress = userDb.Email,
            FirstName = userDb.FirstName,
            LastName = userDb.LastName,
            CreatedBy = userDb.CreatedBy,
            ProfilePictureDataUrl = userDb.ProfilePictureDataUrl,
            CreatedOn = userDb.CreatedOn,
            LastModifiedBy = userDb.LastModifiedBy,
            LastModifiedOn = userDb.LastModifiedOn,
            IsDeleted = userDb.IsDeleted,
            DeletedOn = userDb.DeletedOn,
            AccountType = userDb.AccountType,
            AuthState = userDb.AuthState,
            Notes = userDb.Notes,
            Currency = userDb.Currency
        };
    }

    public static IEnumerable<AppUserSlim> ToSlims(this IEnumerable<AppUserSecurityDb> userDbs)
    {
        return userDbs.Select(x => x.ToSlim()).ToList();
    }

    public static UserBasicResponse ToBasicResponse(this AppUserSecurityDb userDb)
    {
        return new UserBasicResponse
        {
            Id = userDb.Id,
            Username = userDb.Username,
            CreatedOn = userDb.CreatedOn,
            AuthState = userDb.AuthState.ToString(),
            AccountType = userDb.AccountType.ToString()
        };
    }

    public static AppUserFull ToUserFull(this AppUserSecurityDb userDb)
    {
        return new AppUserFull
        {
            Id = userDb.Id,
            Username = userDb.Username,
            EmailAddress = userDb.Email,
            FirstName = userDb.FirstName,
            LastName = userDb.LastName,
            CreatedBy = userDb.CreatedBy,
            ProfilePictureDataUrl = userDb.ProfilePictureDataUrl,
            CreatedOn = userDb.CreatedOn,
            LastModifiedBy = userDb.LastModifiedBy,
            LastModifiedOn = userDb.LastModifiedOn,
            IsDeleted = userDb.IsDeleted,
            DeletedOn = userDb.DeletedOn,
            AccountType = userDb.AccountType,
            Roles = [],
            ExtendedAttributes = [],
            Permissions = [],
            AuthState = userDb.AuthState,
            Notes = userDb.Notes,
            Currency = userDb.Currency
        };
    }

    public static AppUserFullDb ToUserFullDb(this AppUserDb userDb)
    {
        return new AppUserFullDb
        {
            Id = userDb.Id,
            Username = userDb.Username,
            Email = userDb.Email,
            EmailConfirmed = userDb.EmailConfirmed,
            PhoneNumber = userDb.PhoneNumber,
            PhoneNumberConfirmed = userDb.PhoneNumberConfirmed,
            FirstName = userDb.FirstName,
            LastName = userDb.LastName,
            CreatedBy = userDb.CreatedBy,
            ProfilePictureDataUrl = userDb.ProfilePictureDataUrl,
            CreatedOn = userDb.CreatedOn,
            LastModifiedBy = userDb.LastModifiedBy,
            LastModifiedOn = userDb.LastModifiedOn,
            IsDeleted = userDb.IsDeleted,
            DeletedOn = userDb.DeletedOn,
            AccountType = userDb.AccountType,
            Roles = [],
            ExtendedAttributes = [],
            Permissions = [],
            AuthState = AuthState.Unknown,
            Notes = userDb.Notes,
            Currency = userDb.Currency
        };
    }

    public static AppUserCreate ToCreate(this AppUserSecurityFull appUser)
    {
        return new AppUserCreate
        {
            Username = appUser.Username,
            Email = appUser.Email,
            EmailConfirmed = appUser.EmailConfirmed,
            PhoneNumber = appUser.PhoneNumber,
            PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            ProfilePictureDataUrl = appUser.ProfilePictureDataUrl,
            CreatedBy = appUser.CreatedBy,
            CreatedOn = appUser.CreatedOn,
            LastModifiedBy = appUser.LastModifiedBy,
            LastModifiedOn = appUser.LastModifiedOn,
            IsDeleted = appUser.IsDeleted,
            DeletedOn = appUser.DeletedOn,
            AccountType = appUser.AccountType,
            Notes = appUser.Notes,
            Currency = appUser.Currency
        };
    }

    public static UserLoginResponse ToLoginResponse(this LocalStorageRequest localStorage)
    {
        return new UserLoginResponse
        {
            ClientId = localStorage.ClientId ?? "",
            Token = localStorage.Token ?? "",
            RefreshToken = localStorage.RefreshToken ?? "",
            RefreshTokenExpiryTime = DateTime.Now.ToUniversalTime()
        };
    }
}