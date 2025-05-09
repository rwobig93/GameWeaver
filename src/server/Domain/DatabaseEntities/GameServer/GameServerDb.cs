﻿using Domain.Contracts;
using Domain.Enums.GameServer;

namespace Domain.DatabaseEntities.GameServer;

public class GameServerDb : IAuditableEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid HostId { get; set; }
    public Guid GameId { get; set; }
    public Guid GameProfileId { get; set; }
    public Guid? ParentGameProfileId { get; set; }
    public string ServerBuildVersion { get; set; } = "";
    public string ServerName { get; set; } = "";
    public string Password { get; set; } = "";
    public string PasswordRcon { get; set; } = "";
    public string PasswordAdmin { get; set; } = "";
    public string PublicIp { get; set; } = "";
    public string PrivateIp { get; set; } = "";
    public string ExternalHostname { get; set; } = "";
    public int PortGame { get; set; } = 0;
    public int PortPeer { get; set; } = 0;
    public int PortQuery { get; set; } = 0;
    public int PortRcon { get; set; } = 0;
    public bool Modded { get; set; } = false;
    public bool Private { get; set; } = true;
    public ConnectivityState ServerState { get; set; }
    public string RunningConfigHash { get; set; } = "";
    public string StorageConfigHash { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}