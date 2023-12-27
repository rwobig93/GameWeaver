using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.HostRegistration;
using Application.Models.GameServer.WeaverWork;
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
            Description = hostDb.Description,
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

    public static IEnumerable<HostSlim> ToSlims(this IEnumerable<HostDb> hostDbs)
    {
        return hostDbs.Select(x => x.ToSlim()).ToList();
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
            Description = hostDb.Description,
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
            Description = registrationDb.Description,
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

    public static IEnumerable<HostRegistrationFull> ToFulls(this IEnumerable<HostRegistrationDb> hostDbs)
    {
        return hostDbs.Select(x => x.ToFull()).ToList();
    }

    public static HostRegistrationUpdate ToUpdate(this HostRegistrationDb registrationDb)
    {
        return new HostRegistrationUpdate
        {
            Id = registrationDb.Id,
            HostId = registrationDb.HostId,
            Description = registrationDb.Description,
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

    public static HostCheckInFull ToFull(this HostCheckInDb checkInDb)
    {
        return new HostCheckInFull
        {
            Id = checkInDb.Id,
            HostId = checkInDb.HostId,
            SendTimestamp = checkInDb.SendTimestamp,
            ReceiveTimestamp = checkInDb.ReceiveTimestamp,
            CpuUsage = checkInDb.CpuUsage,
            RamUsage = checkInDb.RamUsage,
            Uptime = checkInDb.Uptime,
            NetworkOutMb = checkInDb.NetworkOutMb,
            NetworkInMb = checkInDb.NetworkInMb
        };
    }

    public static IEnumerable<HostCheckInFull> ToFulls(this IEnumerable<HostCheckInDb> checkInDbs)
    {
        return checkInDbs.Select(x => x.ToFull()).ToList();
    }

    public static WeaverWorkClient ToClientWork(this WeaverWorkSlim workSlim)
    {
        return new WeaverWorkClient
        {
            Id = workSlim.Id,
            GameServerId = workSlim.GameServerId,
            TargetType = workSlim.TargetType,
            Status = workSlim.Status,
            WorkData = workSlim.WorkData
        };
    }

    public static IEnumerable<WeaverWorkClient> ToClientWorks(this IEnumerable<WeaverWorkSlim> workSlims)
    {
        return workSlims.Select(x => x.ToClientWork()).ToList();
    }
}