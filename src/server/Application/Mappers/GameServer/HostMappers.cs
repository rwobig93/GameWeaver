using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.HostRegistration;
using Application.Models.GameServer.WeaverWork;
using Application.Requests.GameServer.Host;
using Domain.DatabaseEntities.GameServer;
using Domain.Models.Host;
using MemoryPack;

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
            OsName = hostDb.OsName,
            OsVersion = hostDb.OsVersion,
            AllowedPorts = MemoryPackSerializer.Deserialize<List<string>>(hostDb.AllowedPorts),
            Cpus = MemoryPackSerializer.Deserialize<List<HostCpu>>(hostDb.Cpus),
            Motherboards = MemoryPackSerializer.Deserialize<List<HostMotherboard>>(hostDb.Motherboards),
            Storage = MemoryPackSerializer.Deserialize<List<HostStorage>>(hostDb.Storage),
            NetworkInterfaces = MemoryPackSerializer.Deserialize<List<HostNetworkInterface>>(hostDb.NetworkInterfaces),
            RamModules = MemoryPackSerializer.Deserialize<List<HostRam>>(hostDb.RamModules),
            Gpus = MemoryPackSerializer.Deserialize<List<HostGpu>>(hostDb.Gpus),
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

    public static HostCreateDb ToCreateDb(this HostCreate host)
    {
        return new HostCreateDb
        {
            OwnerId = host.OwnerId,
            PasswordHash = host.PasswordHash,
            PasswordSalt = host.PasswordSalt,
            Hostname = host.Hostname,
            FriendlyName = host.FriendlyName,
            Description = host.Description,
            PrivateIp = host.PrivateIp,
            PublicIp = host.PublicIp,
            CurrentState = host.CurrentState,
            Os = host.Os,
            OsName = "",
            OsVersion = "",
            AllowedPorts = MemoryPackSerializer.Serialize(host.AllowedPorts),
            Cpus = MemoryPackSerializer.Serialize(new List<HostCpu>()),
            Motherboards = MemoryPackSerializer.Serialize(new List<HostMotherboard>()),
            Storage = MemoryPackSerializer.Serialize(new List<HostStorage>()),
            NetworkInterfaces = MemoryPackSerializer.Serialize(new List<HostNetworkInterface>()),
            RamModules = MemoryPackSerializer.Serialize(new List<HostRam>()),
            Gpus = MemoryPackSerializer.Serialize(new List<HostGpu>()),
            CreatedBy = host.CreatedBy,
            CreatedOn = host.CreatedOn,
            LastModifiedBy = host.LastModifiedBy,
            LastModifiedOn = host.LastModifiedOn,
            IsDeleted = host.IsDeleted,
            DeletedOn = host.DeletedOn
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
            Description = hostDb.Description,
            PrivateIp = hostDb.PrivateIp,
            PublicIp = hostDb.PublicIp,
            CurrentState = hostDb.CurrentState,
            Os = hostDb.Os,
            AllowedPorts = MemoryPackSerializer.Deserialize<List<string>>(hostDb.AllowedPorts),
            Cpus = MemoryPackSerializer.Deserialize<List<HostCpu>>(hostDb.Cpus),
            Motherboards = MemoryPackSerializer.Deserialize<List<HostMotherboard>>(hostDb.Motherboards),
            Storage = MemoryPackSerializer.Deserialize<List<HostStorage>>(hostDb.Storage),
            NetworkInterfaces = MemoryPackSerializer.Deserialize<List<HostNetworkInterface>>(hostDb.NetworkInterfaces),
            RamModules = MemoryPackSerializer.Deserialize<List<HostRam>>(hostDb.RamModules),
            Gpus = MemoryPackSerializer.Deserialize<List<HostGpu>>(hostDb.Gpus),
            CreatedBy = hostDb.CreatedBy,
            CreatedOn = hostDb.CreatedOn,
            LastModifiedBy = hostDb.LastModifiedBy,
            LastModifiedOn = hostDb.LastModifiedOn,
            IsDeleted = hostDb.IsDeleted,
            DeletedOn = hostDb.DeletedOn
        };
    }

    public static HostUpdate ToUpdate(this HostDetailUpdate host)
    {
        return new HostUpdate
        {
            Id = host.HostId,
            Hostname = host.Hostname,
            Os = host.Os.Os,
            OsName = host.Os.Name,
            OsVersion = host.Os.Version,
            Cpus = host.Cpus,
            Motherboards = host.Motherboards,
            Storage = host.Storage,
            NetworkInterfaces = host.NetworkInterfaces,
            RamModules = host.RamModules,
            Gpus = host.Gpus
        };
    }

    public static HostUpdateDb ToUpdateDb(this HostUpdate host)
    {
        return new HostUpdateDb
        {
            Id = host.Id,
            OwnerId = host.OwnerId,
            PasswordHash = host.PasswordHash,
            PasswordSalt = host.PasswordSalt,
            Hostname = host.Hostname,
            FriendlyName = host.FriendlyName,
            Description = host.Description,
            PrivateIp = host.PrivateIp,
            PublicIp = host.PublicIp,
            CurrentState = host.CurrentState,
            Os = host.Os,
            OsName = host.OsName,
            OsVersion = host.OsVersion,
            AllowedPorts = host.AllowedPorts is null ? null : MemoryPackSerializer.Serialize(host.AllowedPorts),
            Cpus = host.Cpus is null ? null : MemoryPackSerializer.Serialize(host.Cpus),
            Motherboards = host.Motherboards is null ? null : MemoryPackSerializer.Serialize(host.Motherboards),
            Storage = host.Storage is null ? null : MemoryPackSerializer.Serialize(host.Storage),
            NetworkInterfaces = host.NetworkInterfaces is null ? null : MemoryPackSerializer.Serialize(host.NetworkInterfaces),
            RamModules = host.RamModules is null ? null : MemoryPackSerializer.Serialize(host.RamModules),
            Gpus = host.Gpus is null ? null : MemoryPackSerializer.Serialize(host.Gpus),
            CreatedBy = host.CreatedBy,
            CreatedOn = host.CreatedOn,
            LastModifiedBy = host.LastModifiedBy,
            LastModifiedOn = host.LastModifiedOn,
            IsDeleted = host.IsDeleted,
            DeletedOn = host.DeletedOn
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
            NetworkOutBytes = checkInDb.NetworkOutBytes,
            NetworkInBytes = checkInDb.NetworkInBytes
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

    public static HostCreate ToCreate(this HostCreateRequest request)
    {
        return new HostCreate
        {
            OwnerId = request.OwnerId,
            FriendlyName = request.Name,
            Description = request.Description,
            AllowedPorts = request.AllowedPorts
        };
    }

    public static HostUpdate ToUpdate(this HostUpdateRequest request)
    {
        return new HostUpdate
        {
            Id = request.Id,
            OwnerId = request.OwnerId,
            FriendlyName = request.Name,
            Description = request.Description,
            AllowedPorts = request.AllowedPorts
        };
    }

    public static HostRegistrationUpdate ToUpdate(this HostRegistrationUpdateRequest request)
    {
        return new HostRegistrationUpdate
        {
            Id = request.Id,
            Description = request.Description,
            Active = request.Active
        };
    }
}