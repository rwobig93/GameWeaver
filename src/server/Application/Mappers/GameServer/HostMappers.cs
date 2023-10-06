using Application.Models.GameServer.Host;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;

namespace Application.Mappers.GameServer;

public static class HostMappers
{
    public static HostSlim ToSlim(this HostDb hostDb)
    {
        return new HostSlim
        {
            Id = hostDb.Id,
            OwnerId = hostDb.OwnerId,
            Hostname = hostDb.Hostname,
            FriendlyName = hostDb.FriendlyName,
            PrivateIp = hostDb.PrivateIp,
            PublicIp = hostDb.PublicIp,
            CurrentState = hostDb.CurrentState,
            Os = hostDb.Os,
            AllowedPorts = hostDb.AllowedPorts,
            CreatedBy = hostDb.CreatedBy,
            CreatedOn = hostDb.CreatedOn,
            LastModifiedBy = hostDb.LastModifiedBy,
            LastModifiedOn = hostDb.LastModifiedOn,
            IsDeleted = hostDb.IsDeleted,
            DeletedOn = hostDb.DeletedOn
        };
    }

    public static HostUpdate ToUpdate(this HostDb hostDb)
    {
        return new HostUpdate
        {
            Id = hostDb.Id,
            OwnerId = hostDb.OwnerId,
            PasswordHash = hostDb.PasswordHash,
            PasswordSalt = hostDb.PasswordSalt,
            Hostname = hostDb.Hostname,
            FriendlyName = hostDb.FriendlyName,
            PrivateIp = hostDb.PrivateIp,
            PublicIp = hostDb.PublicIp,
            CurrentState = hostDb.CurrentState,
            Os = hostDb.Os,
            AllowedPorts = hostDb.AllowedPorts,
            CreatedBy = hostDb.CreatedBy,
            CreatedOn = hostDb.CreatedOn,
            LastModifiedBy = hostDb.LastModifiedBy,
            LastModifiedOn = hostDb.LastModifiedOn,
            IsDeleted = hostDb.IsDeleted,
            DeletedOn = hostDb.DeletedOn
        };
    }

    public static HostRegistrationFull ToFull(this HostRegistrationDb registrationDb)
    {
        return new HostRegistrationFull
        {
            Id = registrationDb.Id,
            HostId = registrationDb.HostId,
            Active = registrationDb.Active,
            Key = registrationDb.Key,
            ActivationDate = registrationDb.ActivationDate,
            ActivationPublicIp = registrationDb.ActivationPublicIp,
            CreatedBy = registrationDb.CreatedBy,
            CreatedOn = registrationDb.CreatedOn,
            LastModifiedBy = registrationDb.LastModifiedBy,
            LastModifiedOn = registrationDb.LastModifiedOn
        };
    }

    public static HostRegistrationUpdate ToUpdate(this HostRegistrationDb registrationDb)
    {
        return new HostRegistrationUpdate
        {
            Id = registrationDb.Id,
            HostId = registrationDb.HostId,
            Active = registrationDb.Active,
            Key = registrationDb.Key,
            ActivationDate = registrationDb.ActivationDate,
            ActivationPublicIp = registrationDb.ActivationPublicIp,
            CreatedBy = registrationDb.CreatedBy,
            CreatedOn = registrationDb.CreatedOn,
            LastModifiedBy = registrationDb.LastModifiedBy,
            LastModifiedOn = registrationDb.LastModifiedOn
        };
    }
}