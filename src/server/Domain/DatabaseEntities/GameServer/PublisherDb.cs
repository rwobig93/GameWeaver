﻿namespace Domain.DatabaseEntities.GameServer;

public class PublisherDb
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
}