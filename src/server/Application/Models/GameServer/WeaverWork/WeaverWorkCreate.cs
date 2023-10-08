﻿using Domain.Enums.GameServer;

namespace Application.Models.GameServer.WeaverWork;

public class WeaverWorkCreate
{
    public Guid HostId { get; set; }
    public Guid? GameServerId { get; set; }
    public WeaverWorkTarget TargetType { get; set; }
    public WeaverWorkState Status { get; set; } = WeaverWorkState.WaitingToBePickedUp;
    public string WorkData { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}